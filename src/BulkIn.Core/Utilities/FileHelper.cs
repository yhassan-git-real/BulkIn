using BulkInApp.Core.Configuration;

namespace BulkInApp.Core.Utilities
{
    /// <summary>
    /// Helper class for file operations and discovery
    /// </summary>
    public class FileHelper
    {
        private readonly FileSettings _fileSettings;

        /// <summary>
        /// Initializes a new instance of FileHelper
        /// </summary>
        /// <param name="fileSettings">File configuration settings</param>
        public FileHelper(FileSettings fileSettings)
        {
            _fileSettings = fileSettings ?? throw new ArgumentNullException(nameof(fileSettings));
        }

        /// <summary>
        /// Discovers all files matching the configured pattern(s)
        /// </summary>
        /// <returns>List of file paths to process</returns>
        public List<string> DiscoverFiles()
        {
            if (!Directory.Exists(_fileSettings.SourceFilePath))
            {
                throw new DirectoryNotFoundException(
                    $"Source directory not found: {_fileSettings.SourceFilePath}");
            }

            var allFiles = new List<string>();
            var effectivePatterns = _fileSettings.GetEffectivePatterns();

            // Get all files matching each pattern
            foreach (var pattern in effectivePatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern))
                    continue;

                var matchingFiles = Directory.GetFiles(
                    _fileSettings.SourceFilePath,
                    pattern,
                    SearchOption.TopDirectoryOnly);

                allFiles.AddRange(matchingFiles);
            }

            // Remove duplicates (in case patterns overlap)
            allFiles = allFiles.Distinct().ToList();

            // Filter out excluded patterns
            var filteredFiles = allFiles
                .Where(file => !ShouldExcludeFile(file))
                .ToList();

            // Sort if configured
            if (_fileSettings.ProcessInAlphabeticalOrder)
            {
                filteredFiles = filteredFiles
                    .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            return filteredFiles;
        }

        /// <summary>
        /// Checks if a file should be excluded based on exclude patterns
        /// </summary>
        private bool ShouldExcludeFile(string filePath)
        {
            if (_fileSettings.ExcludeFilePatterns == null || !_fileSettings.ExcludeFilePatterns.Any())
            {
                return false;
            }

            var fileName = Path.GetFileName(filePath);
            
            foreach (var pattern in _fileSettings.ExcludeFilePatterns)
            {
                if (MatchesPattern(fileName, pattern))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a filename matches a wildcard pattern
        /// </summary>
        private bool MatchesPattern(string fileName, string pattern)
        {
            // Convert wildcard pattern to regex
            var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
            
            return System.Text.RegularExpressions.Regex.IsMatch(
                fileName, 
                regexPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Validates that a file exists and is readable
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if file is valid, false otherwise</returns>
        public bool ValidateFile(string filePath, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!File.Exists(filePath))
            {
                errorMessage = $"File not found: {filePath}";
                return false;
            }

            try
            {
                // Try to open file for reading to check if it's accessible
                using (var fs = File.OpenRead(filePath))
                {
                    // File is accessible
                }
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = $"Access denied to file: {filePath}";
                return false;
            }
            catch (IOException ex)
            {
                errorMessage = $"File is locked or inaccessible: {filePath}. Error: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error validating file: {filePath}. Error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Gets the size of a file in bytes
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>File size in bytes</returns>
        public long GetFileSize(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }

        /// <summary>
        /// Gets a human-readable file size string
        /// </summary>
        /// <param name="bytes">Size in bytes</param>
        /// <returns>Formatted file size (e.g., "1.5 MB")</returns>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Counts the number of lines in a file (for estimation)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Approximate line count</returns>
        public long EstimateLineCount(string filePath)
        {
            long lineCount = 0;
            
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    while (reader.ReadLine() != null)
                    {
                        lineCount++;
                    }
                }
            }
            catch
            {
                // Return 0 if unable to count lines
                return 0;
            }

            return lineCount;
        }

        /// <summary>
        /// Gets file information summary
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Formatted file information string</returns>
        public string GetFileInfo(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileName = fileInfo.Name;
            var fileSize = FormatFileSize(fileInfo.Length);
            var createdDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            var modifiedDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

            return $@"
   File: {fileName}
   Path: {filePath}
   Size: {fileSize}
   Created: {createdDate}
   Modified: {modifiedDate}";
        }
    }
}
