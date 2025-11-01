namespace BulkInApp.Models
{
    /// <summary>
    /// Tracks overall processing statistics and metrics
    /// </summary>
    public class FileProcessingStats
    {
        /// <summary>
        /// Total number of files discovered
        /// </summary>
        public int TotalFilesDiscovered { get; set; }

        /// <summary>
        /// Number of files successfully processed
        /// </summary>
        public int FilesProcessedSuccessfully { get; set; }

        /// <summary>
        /// Number of files that failed processing
        /// </summary>
        public int FilesFailed { get; set; }

        /// <summary>
        /// Total number of rows/lines processed across all files
        /// </summary>
        public long TotalRowsProcessed { get; set; }

        /// <summary>
        /// Total processing time for all files
        /// </summary>
        public TimeSpan TotalDuration { get; set; }

        /// <summary>
        /// List of all processing results
        /// </summary>
        public List<ProcessingResult> Results { get; set; } = new List<ProcessingResult>();

        /// <summary>
        /// Timestamp when batch processing started
        /// </summary>
        public DateTime BatchStartTime { get; set; }

        /// <summary>
        /// Timestamp when batch processing completed
        /// </summary>
        public DateTime BatchEndTime { get; set; }

        /// <summary>
        /// Calculates average rows per second across all files
        /// </summary>
        public double AverageRowsPerSecond => TotalDuration.TotalSeconds > 0 
            ? TotalRowsProcessed / TotalDuration.TotalSeconds 
            : 0;

        /// <summary>
        /// Calculates success rate as a percentage
        /// </summary>
        public double SuccessRate => TotalFilesDiscovered > 0 
            ? (FilesProcessedSuccessfully * 100.0) / TotalFilesDiscovered 
            : 0;

        /// <summary>
        /// Total size of all processed files in bytes
        /// </summary>
        public long TotalFileSizeBytes => Results.Sum(r => r.FileSizeBytes);

        /// <summary>
        /// Gets a comprehensive summary report
        /// </summary>
        public string GetComprehensiveSummary()
        {
            var totalSizeMB = TotalFileSizeBytes / (1024.0 * 1024.0);
            var failIcon = FilesFailed > 0 ? " ❌" : "";
            
            var summary = $@"
            📊 Results: {FilesProcessedSuccessfully}/{TotalFilesDiscovered} files successful{failIcon}  •  {SuccessRate:N1}% success rate
            📈 Data:    {TotalRowsProcessed:N0} rows  •  {totalSizeMB:N2} MB  •  {AverageRowsPerSecond:N0} rows/sec
            ⏱️  Time:    {BatchStartTime:HH:mm:ss} → {BatchEndTime:HH:mm:ss}  •  Duration: {TotalDuration.Hours:D2}:{TotalDuration.Minutes:D2}:{TotalDuration.Seconds:D2}";

            if (FilesFailed > 0)
            {
                summary += $"\n\n❌ Failed Files:";
                foreach (var failed in Results.Where(r => !r.Success))
                {
                    summary += $"\n   • {failed.Filename}: {failed.ErrorMessage}";
                }
            }
            
            return summary;
        }

        /// <summary>
        /// Adds a processing result to the statistics
        /// </summary>
        public void AddResult(ProcessingResult result)
        {
            Results.Add(result);
            
            if (result.Success)
            {
                FilesProcessedSuccessfully++;
                TotalRowsProcessed += result.RowsProcessed;
            }
            else
            {
                FilesFailed++;
            }
        }
    }
}
