using System.Text;
using BulkIn.Configuration;

namespace BulkIn.Services
{
    /// <summary>
    /// Service for reading text files with streaming to prevent memory overflow
    /// CRITICAL: Preserves ALL whitespace (leading, trailing, inter-column)
    /// </summary>
    public class FileReaderService
    {
        private readonly ProcessingSettings _settings;

        /// <summary>
        /// Initializes a new instance of FileReaderService
        /// </summary>
        /// <param name="settings">Processing configuration settings</param>
        public FileReaderService(ProcessingSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Reads a file line-by-line using streaming (no full file load)
        /// CRITICAL: Does NOT trim any whitespace - preserves content exactly as written
        /// </summary>
        /// <param name="filePath">Path to the file to read</param>
        /// <param name="progressCallback">Optional callback for progress reporting (line number, line content)</param>
        /// <returns>Enumerable of lines preserving all whitespace</returns>
        public IEnumerable<string> ReadLinesStreaming(string filePath, Action<long, string>? progressCallback = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            long lineNumber = 0;

            using (var fileStream = new FileStream(
                filePath, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read, 
                _settings.StreamReaderBufferSize))
            {
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    string? line;
                    
                    // Read line-by-line without loading entire file into memory
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        
                        // CRITICAL: Do NOT trim whitespace - preserve exactly as read
                        // Even if line is empty or all whitespace, yield it
                        yield return line;

                        // Report progress if callback provided
                        progressCallback?.Invoke(lineNumber, line);
                    }
                }
            }
        }

        /// <summary>
        /// Reads a file with retry logic for file access issues
        /// </summary>
        /// <param name="filePath">Path to the file to read</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <param name="maxRetries">Maximum number of retry attempts (default: 3)</param>
        /// <param name="retryDelaySeconds">Delay between retries in seconds (default: 2)</param>
        /// <returns>Enumerable of lines preserving all whitespace</returns>
        public IEnumerable<string> ReadLinesWithRetry(
            string filePath, 
            Action<long, string>? progressCallback = null,
            int maxRetries = 3,
            int retryDelaySeconds = 2)
        {
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < maxRetries)
            {
                attempt++;

                try
                {
                    // Return the streaming enumerable directly on success
                    return ReadLinesStreaming(filePath, progressCallback);
                }
                catch (IOException ex) when (attempt < maxRetries)
                {
                    lastException = ex;
                    Thread.Sleep(retryDelaySeconds * 1000 * attempt);
                }
                catch (UnauthorizedAccessException ex) when (attempt < maxRetries)
                {
                    lastException = ex;
                    Thread.Sleep(retryDelaySeconds * 1000 * attempt);
                }
            }

            // All retries failed
            throw new InvalidOperationException(
                $"Failed to read file after {maxRetries} attempts: {filePath}",
                lastException);
        }

        /// <summary>
        /// Counts the number of lines in a file efficiently
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Number of lines in the file</returns>
        public long CountLines(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            long lineCount = 0;

            using (var fileStream = new FileStream(
                filePath, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read, 
                _settings.StreamReaderBufferSize))
            {
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    while (reader.ReadLine() != null)
                    {
                        lineCount++;
                    }
                }
            }

            return lineCount;
        }

        /// <summary>
        /// Validates that file content can be read (quick test)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if file is readable, false otherwise</returns>
        public bool ValidateFileReadable(string filePath, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                using (var fileStream = new FileStream(
                    filePath, 
                    FileMode.Open, 
                    FileAccess.Read, 
                    FileShare.Read))
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        // Try to read first line
                        reader.ReadLine();
                    }
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                errorMessage = $"File not found: {filePath}";
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = $"Access denied: {filePath}";
                return false;
            }
            catch (IOException ex)
            {
                errorMessage = $"File is locked or inaccessible: {filePath}. Error: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error reading file: {filePath}. Error: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Gets the encoding of a file
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>Detected encoding</returns>
        public Encoding DetectEncoding(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    // Read a small portion to detect encoding
                    reader.ReadLine();
                    return reader.CurrentEncoding;
                }
            }
        }

        /// <summary>
        /// Reads a file with custom encoding
        /// </summary>
        /// <param name="filePath">Path to the file to read</param>
        /// <param name="encoding">Encoding to use</param>
        /// <param name="progressCallback">Optional callback for progress reporting</param>
        /// <returns>Enumerable of lines preserving all whitespace</returns>
        public IEnumerable<string> ReadLinesWithEncoding(
            string filePath, 
            Encoding encoding,
            Action<long, string>? progressCallback = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            long lineNumber = 0;

            using (var fileStream = new FileStream(
                filePath, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.Read, 
                _settings.StreamReaderBufferSize))
            {
                using (var reader = new StreamReader(fileStream, encoding))
                {
                    string? line;
                    
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        
                        // CRITICAL: Do NOT trim whitespace
                        yield return line;

                        progressCallback?.Invoke(lineNumber, line);
                    }
                }
            }
        }
    }
}
