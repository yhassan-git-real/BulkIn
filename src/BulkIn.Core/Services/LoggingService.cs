using Serilog;
using Serilog.Events;
using BulkInApp.Core.Configuration;
using BulkInApp.Core.Models;
using BulkInApp.Core.Services.Interfaces;

namespace BulkInApp.Core.Services
{
    /// <summary>
    /// Service for comprehensive logging with Serilog and UI event support
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly LoggingSettings _settings;
        private readonly ILogger _successLogger;
        private readonly ILogger _errorLogger;
        private readonly string _sessionId;

        /// <summary>
        /// Event raised when a new log entry is added (for UI integration)
        /// </summary>
        public event EventHandler<LogEntryEventArgs>? LogEntryAdded;

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
        /// Raises the LogEntryAdded event for UI integration
        /// </summary>
        private void RaiseLogEntry(LogLevel level, string message)
        {
            LogEntryAdded?.Invoke(this, new LogEntryEventArgs
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            });
        }

        /// <summary>
        /// Logs the start of a processing session
        /// </summary>
        private void LogSessionStart()
        {
            var fileLogMessage = $@"
╔═══════════════════════════════════════════════════════════╗
║              BulkIn Processing Session Started            ║
╚═══════════════════════════════════════════════════════════╝
Session ID: {_sessionId}
Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Log Directory: {_settings.LogFilePath}
═══════════════════════════════════════════════════════════";

            _successLogger.Information(fileLogMessage);
            RaiseLogEntry(LogLevel.Information, $"Session started: {_sessionId}");
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public void LogInfo(string message)
        {
            _successLogger.Information(message);
            RaiseLogEntry(LogLevel.Information, message);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            _successLogger.Warning(message);
            _errorLogger.Warning(message);
            RaiseLogEntry(LogLevel.Warning, message);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        public void LogError(string message, Exception? exception = null)
        {
            _successLogger.Error(exception, message);
            _errorLogger.Error(exception, message);
            RaiseLogEntry(LogLevel.Error, exception != null ? $"{message}: {exception.Message}" : message);
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
            
            var uiMessage = $"✓ {result.Filename} - {result.RowsProcessed:N0} rows in {result.Duration.TotalSeconds:N1}s";
            RaiseLogEntry(LogLevel.Information, uiMessage);
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
            
            var uiMessage = $"✗ {result.Filename} - FAILED: {result.ErrorMessage}";
            RaiseLogEntry(LogLevel.Error, uiMessage);
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

File List:";

            foreach (var file in files.Select((f, i) => new { Index = i + 1, Path = f }))
            {
                message += $"\n  {file.Index}. {Path.GetFileName(file.Path)}";
            }

            message += "\n═══════════════════════════════════════════════════════════";

            _successLogger.Information(message);
            RaiseLogEntry(LogLevel.Information, $"Found {files.Count} file(s) to process");
        }

        /// <summary>
        /// Logs batch processing start
        /// </summary>
        public void LogBatchStart(int fileCount)
        {
            var message = $"Starting batch processing of {fileCount} file(s)...";
            _successLogger.Information(message);
            RaiseLogEntry(LogLevel.Information, $"Processing {fileCount} file(s)...");
        }

        /// <summary>
        /// Logs batch processing completion
        /// </summary>
        public void LogBatchComplete(FileProcessingStats stats)
        {
            var summary = stats.GetComprehensiveSummary();
            
            _successLogger.Information(summary);
            RaiseLogEntry(LogLevel.Information, $"Batch completed: {stats.FilesProcessedSuccessfully} succeeded, {stats.FilesFailed} failed");

            if (stats.FilesFailed > 0)
            {
                _errorLogger.Warning($"Batch completed with {stats.FilesFailed} failed file(s)");
            }
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
                RaiseLogEntry(LogLevel.Information, message);
            }
            else
            {
                var message = $"✗ Database connection test failed: {errorMessage}";
                _errorLogger.Error(message);
                RaiseLogEntry(LogLevel.Error, message);
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
                RaiseLogEntry(LogLevel.Information, message);
            }
            else
            {
                var message = $"✗ Configuration validation failed:\n{string.Join("\n", errorMessages)}";
                _errorLogger.Error(message);
                RaiseLogEntry(LogLevel.Error, $"Configuration validation failed: {errorMessages.Count} error(s)");
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
            RaiseLogEntry(LogLevel.Information, "Session ended");

            (_successLogger as IDisposable)?.Dispose();
            (_errorLogger as IDisposable)?.Dispose();
        }
    }
}
