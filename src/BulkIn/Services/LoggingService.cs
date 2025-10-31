using Serilog;
using Serilog.Events;
using BulkIn.Configuration;
using BulkIn.Models;

namespace BulkIn.Services
{
    /// <summary>
    /// Service for comprehensive logging with Serilog
    /// </summary>
    public class LoggingService : IDisposable
    {
        private readonly LoggingSettings _settings;
        private readonly ILogger _successLogger;
        private readonly ILogger _errorLogger;
        private readonly ILogger _consoleLogger;
        private readonly string _sessionId;

        /// <summary>
        /// Initializes a new instance of LoggingService
        /// </summary>
        /// <param name="settings">Logging configuration settings</param>
        public LoggingService(LoggingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Ensure log directory exists
            if (!Directory.Exists(settings.LogFilePath))
            {
                Directory.CreateDirectory(settings.LogFilePath);
            }

            // Configure Success Logger (file only)
            var successLogPath = Path.Combine(
                settings.LogFilePath,
                $"{settings.SuccessLogPrefix}_{_sessionId}.txt");

            _successLogger = new LoggerConfiguration()
                .MinimumLevel.Is(GetLogLevel(settings.LogLevel))
                .WriteTo.File(
                    successLogPath,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760) // 10 MB
                .CreateLogger();

            // Configure Error Logger (file only)
            var errorLogPath = Path.Combine(
                settings.LogFilePath,
                $"{settings.ErrorLogPrefix}_{_sessionId}.txt");

            _errorLogger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Warning)
                .WriteTo.File(
                    errorLogPath,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760) // 10 MB
                .CreateLogger();

            // Configure Console Logger (if enabled)
            if (settings.EnableConsoleLogging)
            {
                _consoleLogger = new LoggerConfiguration()
                    .MinimumLevel.Is(GetLogLevel(settings.LogLevel))
                    .WriteTo.Console(
                        outputTemplate: "{Message:lj}{NewLine}")
                    .CreateLogger();
            }
            else
            {
                _consoleLogger = new LoggerConfiguration().CreateLogger();
            }

            LogSessionStart();
        }

        /// <summary>
        /// Converts string log level to Serilog LogEventLevel
        /// </summary>
        private LogEventLevel GetLogLevel(string level)
        {
            return level.ToLower() switch
            {
                "verbose" => LogEventLevel.Verbose,
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }

        /// <summary>
        /// Logs the start of a processing session
        /// </summary>
        private void LogSessionStart()
        {
            var sessionHeader = $@"
╔═══════════════════════════════════════════════════════════╗
║         BulkIn Processing Session Started                 ║
╚═══════════════════════════════════════════════════════════╝
Session ID: {_sessionId}  •  Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Logs: {_settings.LogFilePath}
═══════════════════════════════════════════════════════════";

            var fileLogMessage = $@"
╔═══════════════════════════════════════════════════════════╗
║              BulkIn Processing Session Started            ║
╚═══════════════════════════════════════════════════════════╝
Session ID: {_sessionId}
Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Log Directory: {_settings.LogFilePath}
═══════════════════════════════════════════════════════════";

            _successLogger.Information(fileLogMessage);
            _consoleLogger.Information(sessionHeader);
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public void LogInfo(string message)
        {
            _successLogger.Information(message);
            _consoleLogger.Information(message);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            _successLogger.Warning(message);
            _errorLogger.Warning(message);
            _consoleLogger.Warning(message);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        public void LogError(string message, Exception? exception = null)
        {
            _successLogger.Error(exception, message);
            _errorLogger.Error(exception, message);
            _consoleLogger.Error(exception, message);
        }

        /// <summary>
        /// Logs a file processing success
        /// </summary>
        public void LogFileSuccess(ProcessingResult result)
        {
            var message = $@"
═══════════════════════════════════════════════════════════
✓ FILE PROCESSED SUCCESSFULLY
═══════════════════════════════════════════════════════════
File: {result.Filename}
Path: {result.FilePath}
Size: {Utilities.FileHelper.FormatFileSize(result.FileSizeBytes)}
Rows Processed: {result.RowsProcessed:N0}
Duration: {result.Duration.TotalSeconds:N2} seconds
Speed: {result.RowsPerSecond:N0} rows/second
Start Time: {result.StartTime:yyyy-MM-dd HH:mm:ss}
End Time: {result.EndTime:yyyy-MM-dd HH:mm:ss}
═══════════════════════════════════════════════════════════";

            _successLogger.Information(message);
            
            var consoleMessage = $"✓ {result.Filename} - {result.RowsProcessed:N0} rows in {result.Duration.TotalSeconds:N1}s";
            _consoleLogger.Information(consoleMessage);
        }

        /// <summary>
        /// Logs a file processing failure
        /// </summary>
        public void LogFileFailure(ProcessingResult result)
        {
            var message = $@"
═══════════════════════════════════════════════════════════
✗ FILE PROCESSING FAILED
═══════════════════════════════════════════════════════════
File: {result.Filename}
Path: {result.FilePath}
Size: {Utilities.FileHelper.FormatFileSize(result.FileSizeBytes)}
Error: {result.ErrorMessage}
Duration: {result.Duration.TotalSeconds:N2} seconds
Start Time: {result.StartTime:yyyy-MM-dd HH:mm:ss}
End Time: {result.EndTime:yyyy-MM-dd HH:mm:ss}
═══════════════════════════════════════════════════════════";

            _errorLogger.Error(result.Exception, message);
            
            var consoleMessage = $"✗ {result.Filename} - FAILED: {result.ErrorMessage}";
            _consoleLogger.Error(consoleMessage);
        }

        /// <summary>
        /// Logs file discovery information
        /// </summary>
        public void LogFilesDiscovered(List<string> files)
        {
            var message = $@"
═══════════════════════════════════════════════════════════
📁 FILES DISCOVERED
═══════════════════════════════════════════════════════════
Total Files Found: {files.Count}
Source Directory: {_settings.LogFilePath}

File List:";

            foreach (var file in files.Select((f, i) => new { Index = i + 1, Path = f }))
            {
                message += $"\n  {file.Index}. {Path.GetFileName(file.Path)}";
            }

            message += "\n═══════════════════════════════════════════════════════════";

            _successLogger.Information(message);
            _consoleLogger.Information($"Found {files.Count} file(s) to process");
        }

        /// <summary>
        /// Logs batch processing start
        /// </summary>
        public void LogBatchStart(int fileCount)
        {
            var message = $"Starting batch processing of {fileCount} file(s)...";
            _successLogger.Information(message);
            _consoleLogger.Information($"Processing {fileCount} file(s)...");
        }

        /// <summary>
        /// Logs batch processing completion
        /// </summary>
        public void LogBatchComplete(FileProcessingStats stats)
        {
            // Add visual separator for summary section
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine(" 📊 FINAL SUMMARY");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            
            var summary = stats.GetComprehensiveSummary();
            
            _successLogger.Information(summary);
            _consoleLogger.Information(summary);

            if (stats.FilesFailed > 0)
            {
                _errorLogger.Warning($"Batch completed with {stats.FilesFailed} failed file(s)");
            }
        }

        /// <summary>
        /// Logs progress during file processing
        /// </summary>
        public void LogProgress(string filename, int currentFile, int totalFiles, long rowsProcessed)
        {
            var percentage = (currentFile * 100.0) / totalFiles;
            var message = $"[{currentFile}/{totalFiles}] ({percentage:N1}%) Processing: {filename} - {rowsProcessed:N0} rows...";
            
            _consoleLogger.Information(message);
        }

        /// <summary>
        /// Logs database connection test result
        /// </summary>
        public void LogDatabaseConnectionTest(bool success, string errorMessage = "")
        {
            if (success)
            {
                var message = "✓ Database connection test successful";
                _successLogger.Information(message);
                _consoleLogger.Information(message);
            }
            else
            {
                var message = $"✗ Database connection test failed: {errorMessage}";
                _errorLogger.Error(message);
                _consoleLogger.Error(message);
            }
        }

        /// <summary>
        /// Logs configuration validation result
        /// </summary>
        public void LogConfigurationValidation(bool success, List<string> errorMessages)
        {
            if (success)
            {
                var message = "✓ Configuration validation successful";
                _successLogger.Information(message);
                _consoleLogger.Information(message);
            }
            else
            {
                var message = $"✗ Configuration validation failed:\n{string.Join("\n", errorMessages)}";
                _errorLogger.Error(message);
                _consoleLogger.Error(message);
            }
        }

        /// <summary>
        /// Disposes the logging service and flushes logs
        /// </summary>
        public void Dispose()
        {
            var message = $@"
═══════════════════════════════════════════════════════════
Session Ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
═══════════════════════════════════════════════════════════";

            _successLogger.Information(message);
            _consoleLogger.Information(message);

            (_successLogger as IDisposable)?.Dispose();
            (_errorLogger as IDisposable)?.Dispose();
            (_consoleLogger as IDisposable)?.Dispose();
        }
    }
}
