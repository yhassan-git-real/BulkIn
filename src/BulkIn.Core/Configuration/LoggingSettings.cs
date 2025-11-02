namespace BulkInApp.Core.Configuration
{
    /// <summary>
    /// Logging configuration settings
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Directory path for log files
        /// </summary>
        public string LogFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Prefix for success log files (e.g., SuccessLog_YYYYMMDD_HHMMSS.txt)
        /// </summary>
        public string SuccessLogPrefix { get; set; } = "SuccessLog";

        /// <summary>
        /// Prefix for error log files (e.g., ErrorLog_YYYYMMDD_HHMMSS.txt)
        /// </summary>
        public string ErrorLogPrefix { get; set; } = "ErrorLog";

        /// <summary>
        /// Enable console logging in addition to file logging
        /// </summary>
        public bool EnableConsoleLogging { get; set; } = true;

        /// <summary>
        /// Logging level (Information, Warning, Error, Debug)
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// Validates that all required settings are provided
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(LogFilePath))
            {
                errorMessage = "LogFilePath is required in LoggingSettings.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(SuccessLogPrefix))
            {
                errorMessage = "SuccessLogPrefix is required in LoggingSettings.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ErrorLogPrefix))
            {
                errorMessage = "ErrorLogPrefix is required in LoggingSettings.";
                return false;
            }

            var validLogLevels = new[] { "Verbose", "Debug", "Information", "Warning", "Error", "Fatal" };
            if (!validLogLevels.Contains(LogLevel, StringComparer.OrdinalIgnoreCase))
            {
                errorMessage = $"LogLevel must be one of: {string.Join(", ", validLogLevels)}";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
