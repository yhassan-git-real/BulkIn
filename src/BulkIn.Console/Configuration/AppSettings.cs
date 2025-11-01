namespace BulkInApp.Configuration
{
    /// <summary>
    /// Root application settings model that contains all configuration sections
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Database connection and configuration settings
        /// </summary>
        public DatabaseSettings DatabaseSettings { get; set; } = new DatabaseSettings();

        /// <summary>
        /// File processing configuration settings
        /// </summary>
        public FileSettings FileSettings { get; set; } = new FileSettings();

        /// <summary>
        /// Data processing and performance settings
        /// </summary>
        public ProcessingSettings ProcessingSettings { get; set; } = new ProcessingSettings();

        /// <summary>
        /// Logging configuration settings
        /// </summary>
        public LoggingSettings LoggingSettings { get; set; } = new LoggingSettings();

        /// <summary>
        /// Validates all configuration sections
        /// </summary>
        public bool IsValid(out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            if (!DatabaseSettings.IsValid(out string dbError))
            {
                errorMessages.Add($"[DatabaseSettings] {dbError}");
            }

            if (!FileSettings.IsValid(out string fileError))
            {
                errorMessages.Add($"[FileSettings] {fileError}");
            }

            if (!ProcessingSettings.IsValid(out string procError))
            {
                errorMessages.Add($"[ProcessingSettings] {procError}");
            }

            if (!LoggingSettings.IsValid(out string logError))
            {
                errorMessages.Add($"[LoggingSettings] {logError}");
            }

            return errorMessages.Count == 0;
        }
    }
}
