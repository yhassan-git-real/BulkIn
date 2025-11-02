namespace BulkInApp.Core.Models
{
    /// <summary>
    /// Tracks the processing result for a single file
    /// </summary>
    public class ProcessingResult
    {
        /// <summary>
        /// Name of the file processed
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Full path to the file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Whether the file was processed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of rows/lines processed from the file
        /// </summary>
        public long RowsProcessed { get; set; }

        /// <summary>
        /// Time taken to process the file
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Exception details if an error occurred
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Timestamp when processing started
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Timestamp when processing completed
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Calculates the processing speed in rows per second
        /// </summary>
        public double RowsPerSecond => Duration.TotalSeconds > 0 
            ? RowsProcessed / Duration.TotalSeconds 
            : 0;

        /// <summary>
        /// Gets a formatted summary of the processing result
        /// </summary>
        public string GetSummary()
        {
            var status = Success ? "✓ SUCCESS" : "✗ FAILED";
            var fileSizeMB = FileSizeBytes / (1024.0 * 1024.0);
            
            var summary = $@"
═══════════════════════════════════════════════════════════
{status}
═══════════════════════════════════════════════════════════
File: {Filename}
Path: {FilePath}
Size: {fileSizeMB:N2} MB
Rows Processed: {RowsProcessed:N0}
Duration: {Duration.TotalSeconds:N2} seconds
Speed: {RowsPerSecond:N0} rows/second
Start Time: {StartTime:yyyy-MM-dd HH:mm:ss}
End Time: {EndTime:yyyy-MM-dd HH:mm:ss}";

            if (!Success && !string.IsNullOrEmpty(ErrorMessage))
            {
                summary += $@"
Error: {ErrorMessage}";
            }

            summary += @"
═══════════════════════════════════════════════════════════";

            return summary;
        }
    }
}
