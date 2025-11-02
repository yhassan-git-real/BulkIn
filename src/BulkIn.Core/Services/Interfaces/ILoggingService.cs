using BulkInApp.Core.Models;

namespace BulkInApp.Core.Services.Interfaces
{
    /// <summary>
    /// Event arguments for log entry added event
    /// </summary>
    public class LogEntryEventArgs : EventArgs
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Log level enumeration
    /// </summary>
    public enum LogLevel
    {
        Verbose,
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }

    /// <summary>
    /// Interface for logging operations with UI event support
    /// </summary>
    public interface ILoggingService : IDisposable
    {
        /// <summary>
        /// Event fired when a new log entry is added (for UI integration)
        /// </summary>
        event EventHandler<LogEntryEventArgs>? LogEntryAdded;

        /// <summary>
        /// Logs an informational message
        /// </summary>
        void LogInfo(string message);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Logs an error message
        /// </summary>
        void LogError(string message, Exception? exception = null);

        /// <summary>
        /// Logs a file processing success
        /// </summary>
        void LogFileSuccess(ProcessingResult result);

        /// <summary>
        /// Logs a file processing failure
        /// </summary>
        void LogFileFailure(ProcessingResult result);

        /// <summary>
        /// Logs batch processing completion
        /// </summary>
        void LogBatchComplete(FileProcessingStats stats);

        /// <summary>
        /// Logs file discovery information
        /// </summary>
        void LogFilesDiscovered(List<string> files);

        /// <summary>
        /// Logs batch processing start
        /// </summary>
        void LogBatchStart(int fileCount);

        /// <summary>
        /// Logs database connection test result
        /// </summary>
        void LogDatabaseConnectionTest(bool success, string errorMessage = "");

        /// <summary>
        /// Logs configuration validation result
        /// </summary>
        void LogConfigurationValidation(bool success, List<string> errorMessages);
    }
}
