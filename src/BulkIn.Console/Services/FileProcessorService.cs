using Microsoft.Data.SqlClient;
using System.Diagnostics;
using BulkInApp.Configuration;
using BulkInApp.Models;
using BulkInApp.Utilities;
using Console = System.Console;

namespace BulkInApp.Services
{
    /// <summary>
    /// Main orchestrator service for file processing workflow
    /// Coordinates: File Reading → Bulk Insert → Data Transfer → Cleanup
    /// </summary>
    public class FileProcessorService : IFileProcessor
    {
        private readonly AppSettings _settings;
        private readonly FileReaderService _fileReaderService;
        private readonly BulkInsertService _bulkInsertService;
        private readonly DataTransferService _dataTransferService;
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly FileHelper _fileHelper;
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Initializes a new instance of FileProcessorService
        /// </summary>
        public FileProcessorService(
            AppSettings settings,
            FileReaderService fileReaderService,
            BulkInsertService bulkInsertService,
            DataTransferService dataTransferService,
            SqlConnectionFactory connectionFactory,
            FileHelper fileHelper,
            LoggingService loggingService)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _fileReaderService = fileReaderService ?? throw new ArgumentNullException(nameof(fileReaderService));
            _bulkInsertService = bulkInsertService ?? throw new ArgumentNullException(nameof(bulkInsertService));
            _dataTransferService = dataTransferService ?? throw new ArgumentNullException(nameof(dataTransferService));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _fileHelper = fileHelper ?? throw new ArgumentNullException(nameof(fileHelper));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        /// <summary>
        /// Validates all prerequisites before processing
        /// </summary>
        public bool ValidatePrerequisites(out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            // Test database connection
            if (!_connectionFactory.TestConnection(out string dbError))
            {
                errorMessages.Add($"Database connection failed: {dbError}");
                return false; // No point continuing if we can't connect
            }

            // Validate target table exists
            if (!_connectionFactory.TableExists(_settings.DatabaseSettings.TargetTableName))
            {
                errorMessages.Add($"Target table '{_settings.DatabaseSettings.TargetTableName}' does not exist.");
            }

            // Ensure temp table exists with correct structure (drop and recreate)
            try
            {
                Console.Write("✅ Ensuring temp table ready...");
                _loggingService.LogInfo($"Ensuring temp table '{_settings.DatabaseSettings.TempTableName}' exists with correct structure...");
                _connectionFactory.EnsureTempTableExists(_settings.DatabaseSettings.TempTableName);
                _loggingService.LogInfo($"✓ Temp table '{_settings.DatabaseSettings.TempTableName}' ready");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" ❌");
                errorMessages.Add($"Failed to create temp table '{_settings.DatabaseSettings.TempTableName}': {ex.Message}");
            }

            // Validate source file path exists
            if (!Directory.Exists(_settings.FileSettings.SourceFilePath))
            {
                errorMessages.Add($"Source file path does not exist: {_settings.FileSettings.SourceFilePath}");
            }

            return errorMessages.Count == 0;
        }

        /// <summary>
        /// Processes a single file through the complete workflow
        /// </summary>
        public async Task<ProcessingResult> ProcessFileAsync(string filePath)
        {
            var result = new ProcessingResult
            {
                FilePath = filePath,
                Filename = Path.GetFileName(filePath),
                StartTime = DateTime.Now,
                FileSizeBytes = _fileHelper.GetFileSize(filePath)
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Validate file is readable
                if (!_fileHelper.ValidateFile(filePath, out string fileError))
                {
                    throw new InvalidOperationException($"File validation failed: {fileError}");
                }

                // Step 1: Read file and bulk insert into temp table
                long rowsInserted = await Task.Run(() => ProcessFileToTempTable(filePath));

                // Step 2: Transfer from temp to target table
                long rowsTransferred = await Task.Run(() => TransferToTargetTable(result.Filename, rowsInserted));

                // Update result
                result.RowsProcessed = rowsTransferred;
                result.Success = true;
                result.EndTime = DateTime.Now;
                result.Duration = stopwatch.Elapsed;

                // Log success
                _loggingService.LogFileSuccess(result);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.Exception = ex;
                result.EndTime = DateTime.Now;
                result.Duration = stopwatch.Elapsed;

                // Log failure
                _loggingService.LogFileFailure(result);
            }

            return result;
        }

        /// <summary>
        /// Processes file data into temporary table
        /// </summary>
        private long ProcessFileToTempTable(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            long rowsInserted = 0;
            long lastReportedRows = 0;

            // Recreate temp table to ensure clean state
            Console.Write($"   {ConsoleColors.Info("🔧 Preparing temp table...")}");
            _connectionFactory.EnsureTempTableExists(_settings.DatabaseSettings.TempTableName);
            Console.WriteLine($" {ConsoleColors.Success("✅")}");

            _loggingService.LogInfo($"   ├─ Preparing temp table...");

            // Read file lines with streaming
            var lines = _fileReaderService.ReadLinesWithRetry(filePath);

            // Use transaction if configured
            if (_settings.ProcessingSettings.EnableTransactionPerFile)
            {
                using (var connection = _connectionFactory.CreateConnectionWithRetry())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Bulk insert with progress tracking
                            rowsInserted = _bulkInsertService.BulkInsertWithProgress(
                                lines,
                                _settings.DatabaseSettings.TempTableName,
                                filename, // Pass the filename
                                (rows) =>
                                {
                                    // Report progress every batch - console only (inline)
                                    if (rows - lastReportedRows >= _settings.ProcessingSettings.BatchSize)
                                    {
                                        Console.Write($"\r   {ConsoleColors.Info("📊 Processing:")} {ConsoleColors.ColorBold(rows.ToString("N0"), ConsoleColors.BrightYellow)} rows...    ");
                                        lastReportedRows = rows;
                                    }
                                },
                                transaction);

                            transaction.Commit();
                            Console.WriteLine($"\r   {ConsoleColors.Success("📥 Inserted:")} {ConsoleColors.ColorBold(rowsInserted.ToString("N0"), ConsoleColors.BrightGreen)} rows          ");
                            _loggingService.LogInfo($"   ├─ Bulk insert complete: {rowsInserted:N0} rows");
                        }
                        catch
                        {
                            transaction.Rollback();
                            Console.WriteLine($"\r   {ConsoleColors.Error("❌ Insert failed - rolled back")}          ");
                            _loggingService.LogError($"   ├─ Bulk insert failed - transaction rolled back");
                            throw;
                        }
                    }
                }
            }
            else
            {
                // No transaction
                rowsInserted = _bulkInsertService.BulkInsertWithProgress(
                    lines,
                    _settings.DatabaseSettings.TempTableName,
                    filename, // Pass the filename
                    (rows) =>
                    {
                        if (rows - lastReportedRows >= _settings.ProcessingSettings.BatchSize)
                        {
                            Console.Write($"\r   {ConsoleColors.Info("📊 Processing:")} {ConsoleColors.ColorBold(rows.ToString("N0"), ConsoleColors.BrightYellow)} rows...    ");
                            lastReportedRows = rows;
                        }
                    });

                Console.WriteLine($"\r   {ConsoleColors.Success("📥 Inserted:")} {ConsoleColors.ColorBold(rowsInserted.ToString("N0"), ConsoleColors.BrightGreen)} rows          ");
                _loggingService.LogInfo($"   ├─ Bulk insert complete: {rowsInserted:N0} rows");
            }

            return rowsInserted;
        }

        /// <summary>
        /// Transfers data from temp table to target table
        /// </summary>
        private long TransferToTargetTable(string filename, long expectedRowCount)
        {
            Console.Write($"   {ConsoleColors.Info("🔄 Transferring to target...")}");
            _loggingService.LogInfo($"   ├─ Transferring to target table...");

            // Execute transfer with validation
            var transferResult = _dataTransferService.ExecuteTransferWorkflow(
                filename,
                expectedRowCount);

            if (!transferResult.Success)
            {
                Console.WriteLine($" {ConsoleColors.Error("❌")}");
                throw new InvalidOperationException($"Data transfer failed: {transferResult.Message}");
            }

            Console.WriteLine($" {ConsoleColors.Success("✅")} ({ConsoleColors.ColorBold(transferResult.RowsTransferred.ToString("N0"), ConsoleColors.BrightGreen)} rows)");
            _loggingService.LogInfo($"   ├─ Transfer complete: {transferResult.RowsTransferred:N0} rows");

            return transferResult.RowsTransferred;
        }

        /// <summary>
        /// Processes multiple files sequentially
        /// </summary>
        public async Task<FileProcessingStats> ProcessFilesAsync(List<string> filePaths)
        {
            var stats = new FileProcessingStats
            {
                TotalFilesDiscovered = filePaths.Count,
                BatchStartTime = DateTime.Now
            };

            _loggingService.LogFilesDiscovered(filePaths);
            _loggingService.LogBatchStart(filePaths.Count);

            var batchStopwatch = Stopwatch.StartNew();

            for (int i = 0; i < filePaths.Count; i++)
            {
                var filePath = filePaths[i];
                var filename = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);
                var fileSize = FileHelper.FormatFileSize(fileInfo.Length);

                // Print clean file header with separator lines and colors
                var timestampStr = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                
                Console.WriteLine();
                Console.WriteLine(ConsoleColors.Secondary("───────────────────────────────────────────────────────────"));
                Console.WriteLine($"📄 {ConsoleColors.ColorBold($"[{i + 1}/{filePaths.Count}]", ConsoleColors.BrightCyan)} {ConsoleColors.Info(filename)}");
                Console.WriteLine($"   {ConsoleColors.Secondary(fileSize)} {ConsoleColors.Secondary("•")} {ConsoleColors.Secondary(timestampStr)}");
                Console.WriteLine(ConsoleColors.Secondary("───────────────────────────────────────────────────────────"));
                
                _loggingService.LogInfo($"[{i + 1}/{filePaths.Count}] Processing: {filename}");

                // Process file
                var result = await ProcessFileAsync(filePath);
                stats.AddResult(result);

                // Display result summary on same line as completion
                if (result.Success)
                {
                    Console.WriteLine($"   {ConsoleColors.Success("✅ Completed:")} {ConsoleColors.ColorBold(result.RowsProcessed.ToString("N0"), ConsoleColors.BrightGreen)} rows {ConsoleColors.Secondary("•")} {ConsoleColors.Info(result.Duration.TotalSeconds.ToString("F1") + "s")} {ConsoleColors.Secondary("•")} {ConsoleColors.Highlight(result.RowsPerSecond.ToString("N0") + " rows/sec")}");
                }
                else
                {
                    Console.WriteLine($"   {ConsoleColors.Error("❌ Failed:")} {result.ErrorMessage}");
                }

                // Continue or stop based on configuration
                if (!result.Success && !_settings.ProcessingSettings.ContinueOnError)
                {
                    _loggingService.LogError($"Processing stopped due to error in file: {filename}");
                    break;
                }
            }

            batchStopwatch.Stop();
            stats.TotalDuration = batchStopwatch.Elapsed;
            stats.BatchEndTime = DateTime.Now;

            // Log final summary
            _loggingService.LogBatchComplete(stats);

            return stats;
        }
    }
}
