namespace BulkInApp.Configuration
{
    /// <summary>
    /// Data processing and performance configuration settings
    /// </summary>
    public class ProcessingSettings
    {
        /// <summary>
        /// Number of rows to process in each batch for bulk insert (default: 50,000)
        /// </summary>
        public int BatchSize { get; set; } = 50000;

        /// <summary>
        /// Maximum degree of parallelism for processing (1 = sequential)
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = 1;

        /// <summary>
        /// Enable transaction per file for rollback capability (recommended: true)
        /// </summary>
        public bool EnableTransactionPerFile { get; set; } = true;

        /// <summary>
        /// Continue processing remaining files if one file fails (true) or stop immediately (false)
        /// </summary>
        public bool ContinueOnError { get; set; } = true;

        /// <summary>
        /// StreamReader buffer size in bytes (default: 65536 = 64 KB)
        /// </summary>
        public int StreamReaderBufferSize { get; set; } = 65536;

        /// <summary>
        /// Validates that all settings have valid values
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (BatchSize <= 0)
            {
                errorMessage = "BatchSize must be greater than 0.";
                return false;
            }

            if (BatchSize > 1000000)
            {
                errorMessage = "BatchSize is too large (max: 1,000,000). Consider using smaller batches.";
                return false;
            }

            if (MaxDegreeOfParallelism < 1)
            {
                errorMessage = "MaxDegreeOfParallelism must be at least 1.";
                return false;
            }

            if (StreamReaderBufferSize < 1024)
            {
                errorMessage = "StreamReaderBufferSize must be at least 1024 bytes (1 KB).";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
