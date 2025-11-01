namespace BulkInApp.Configuration
{
    /// <summary>
    /// File processing configuration settings
    /// </summary>
    public class FileSettings
    {
        /// <summary>
        /// Source directory path containing text files to process
        /// </summary>
        public string SourceFilePath { get; set; } = string.Empty;

        /// <summary>
        /// File pattern for matching files (e.g., *.txt, *.csv) - single pattern for backward compatibility
        /// </summary>
        public string FilePattern { get; set; } = "*.txt";

        /// <summary>
        /// Multiple file patterns for matching files (e.g., ["*.txt", "*.csv", "*.log"])
        /// If specified, this takes precedence over FilePattern
        /// </summary>
        public List<string> FilePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Process files in alphabetical order (true) or discovery order (false)
        /// </summary>
        public bool ProcessInAlphabeticalOrder { get; set; } = true;

        /// <summary>
        /// List of file patterns to exclude from processing
        /// </summary>
        public List<string> ExcludeFilePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Gets the effective file patterns to use (FilePatterns if specified, otherwise FilePattern)
        /// </summary>
        public List<string> GetEffectivePatterns()
        {
            if (FilePatterns != null && FilePatterns.Any())
            {
                return FilePatterns;
            }
            
            return new List<string> { FilePattern };
        }

        /// <summary>
        /// Validates that all required settings are provided
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(SourceFilePath))
            {
                errorMessage = "SourceFilePath is required in FileSettings.";
                return false;
            }

            if (!Directory.Exists(SourceFilePath))
            {
                errorMessage = $"SourceFilePath does not exist: {SourceFilePath}";
                return false;
            }

            var effectivePatterns = GetEffectivePatterns();
            if (!effectivePatterns.Any() || effectivePatterns.All(string.IsNullOrWhiteSpace))
            {
                errorMessage = "At least one FilePattern is required in FileSettings.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
