using Microsoft.Data.SqlClient;
using System.Diagnostics;
using BulkInApp.Core.Configuration;
using BulkInApp.Core.Models;
using BulkInApp.Core.Utilities;
using BulkInApp.Core.Services.Interfaces;

namespace BulkInApp.Core.Services
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
        private readonly ILoggingService _loggingService;
        private readonly IProgressReporter? _progressReporter;

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
            ILoggingService loggingService,
            IProgressReporter? progressReporter = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _fileReaderService = fileReaderService ?? throw new ArgumentNullException(nameof(fileReaderService));
            _bulkInsertService = bulkInsertService ?? throw new ArgumentNullException(nameof(bulkInsertService));
            _dataTransferService = dataTransferService ?? throw new ArgumentNullException(nameof(dataTransferService));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _fileHelper = fileHelper ?? throw new ArgumentNullException(nameof(fileHelper));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _progressReporter = progressReporter;
        }

        /// <summary>
        /// Validates all prerequisites before processing
        /// </summary>
        public bool ValidatePrerequisites(out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            _progressReporter?.ReportStatus(ProcessingStatus.Idle, "Validating prerequisites...");

            // Test database connection
            if (!_connectionFactory.TestConnection(out string dbError))
            {
                errorMessages.Add($"Database connection failed: {dbError}");
                _progressReporter?.ReportStatus(ProcessingStatus.Error, "Database connection failed");
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
                _loggingService.LogInfo($"Ensuring temp table '{_settings.DatabaseSettings.TempTableName}' exists with correct structure...");
                _connectionFactory.EnsureTempTableExists(_settings.DatabaseSettings.TempTableName);
                _loggingService.LogInfo($"✓ Temp table '{_settings.DatabaseSettings.TempTableName}' ready");
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Failed to create temp table '{_settings.DatabaseSettings.TempTableName}': {ex.Message}");
            }

            // Validate source file path exists
            if (!Directory.Exists(_settings.FileSettings.SourceFilePath))
            {
                errorMessages.Add($"Source file path does not exist: {_settings.FileSettings.SourceFilePath}");
            }

            var isValid = errorMessages.Count == 0;
            if (isValid)
            {
                _progressReporter?.ReportStatus(ProcessingStatus.Idle, "Prerequisites validated successfully");
            }
            else
            {
                _progressReporter?.ReportStatus(ProcessingStatus.Error, $"Validation failed: {errorMessages.Count} error(s)");
            }

            return isValid;
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
            _loggingService.LogInfo($"   ├─ Preparing temp table...");
            _connectionFactory.EnsureTempTableExists(_settings.DatabaseSettings.TempTableName);
            _loggingService.LogInfo($"   ├─ Temp table ready");

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
                                    // Report progress via progress reporter (for UI)
                                    if (rows - lastReportedRows >= _settings.ProcessingSettings.BatchSize)
                                    {
                                        _progressReporter?.ReportProgress(
                                            fileIndex: 0,
                                            totalFiles: 1,
                                            fileName: filename,
                                            rowsProcessed: rows,
                                            speed: 0); // Speed calculation would require timing
                                        lastReportedRows = rows;
                                    }
                                },
                                transaction);

                            transaction.Commit();
                            _loggingService.LogInfo($"   ├─ Bulk insert complete: {rowsInserted:N0} rows");
                        }
                        catch
                        {
                            transaction.Rollback();
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
                            _progressReporter?.ReportProgress(
                                fileIndex: 0,
                                totalFiles: 1,
                                fileName: filename,
                                rowsProcessed: rows,
                                speed: 0);
                            lastReportedRows = rows;
                        }
                    });

                _loggingService.LogInfo($"   ├─ Bulk insert complete: {rowsInserted:N0} rows");
            }

            return rowsInserted;
        }

        /// <summary>
        /// Transfers data from temp table to target table
        /// </summary>
        private long TransferToTargetTable(string filename, long expectedRowCount)
        {
            _loggingService.LogInfo($"   ├─ Transferring to target table...");

            // Execute transfer with validation
            var transferResult = _dataTransferService.ExecuteTransferWorkflow(
                filename,
                expectedRowCount);

            if (!transferResult.Success)
            {
                throw new InvalidOperationException($"Data transfer failed: {transferResult.Message}");
            }

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
            _progressReporter?.ReportStatus(ProcessingStatus.Running, $"Processing {filePaths.Count} file(s)");

            var batchStopwatch = Stopwatch.StartNew();
            long totalRowsProcessedSoFar = 0;

            for (int i = 0; i < filePaths.Count; i++)
            {
                var filePath = filePaths[i];
                var filename = Path.GetFileName(filePath);
                var fileInfo = new FileInfo(filePath);

                _loggingService.LogInfo($"[{i + 1}/{filePaths.Count}] Processing: {filename}");
                _progressReporter?.ReportStatus(
                    ProcessingStatus.Running,
                    $"Processing file {i + 1} of {filePaths.Count}: {filename}");

                // Process file
                var fileStartTime = Stopwatch.StartNew();
                var result = await ProcessFileAsync(filePath);
                fileStartTime.Stop();
                
                stats.AddResult(result);

                // Update progress
                if (result.Success)
                {
                    totalRowsProcessedSoFar += result.RowsProcessed;
                    var currentSpeed = result.RowsProcessed / fileStartTime.Elapsed.TotalSeconds;
                    
                    _progressReporter?.ReportProgress(
                        fileIndex: i + 1,
                        totalFiles: filePaths.Count,
                        fileName: filename,
                        rowsProcessed: result.RowsProcessed,
                        speed: currentSpeed);

                    _loggingService.LogInfo(
                        $"   ✅ Completed: {result.RowsProcessed:N0} rows • {result.Duration.TotalSeconds:F1}s • {result.RowsPerSecond:N0} rows/sec");
                }
                else
                {
                    _loggingService.LogError($"   ❌ Failed: {result.ErrorMessage}");
                }

                // Continue or stop based on configuration
                if (!result.Success && !_settings.ProcessingSettings.ContinueOnError)
                {
                    _loggingService.LogError($"Processing stopped due to error in file: {filename}");
                    _progressReporter?.ReportStatus(ProcessingStatus.Error, $"Stopped due to error: {result.ErrorMessage}");
                    break;
                }
            }

            batchStopwatch.Stop();
            stats.TotalDuration = batchStopwatch.Elapsed;
            stats.BatchEndTime = DateTime.Now;

            // Log final summary
            _loggingService.LogBatchComplete(stats);
            _progressReporter?.ReportStatus(
                stats.FilesFailed == 0 ? ProcessingStatus.Completed : ProcessingStatus.Error,
                $"Completed: {stats.FilesProcessedSuccessfully} succeeded, {stats.FilesFailed} failed");

            return stats;
        }
    }
}
