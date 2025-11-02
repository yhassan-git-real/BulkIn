namespace BulkInApp.Core.Services.Interfaces
{
    /// <summary>
    /// Processing status enumeration
    /// </summary>
    public enum ProcessingStatus
    {
        Idle,
        Discovering,
        Running,
        Paused,
        Stopping,
        Stopped,
        Completed,
        Error
    }

    /// <summary>
    /// Event arguments for progress changed event
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        public int CurrentFileIndex { get; set; }
        public int TotalFiles { get; set; }
        public string CurrentFileName { get; set; } = string.Empty;
        public long RowsProcessed { get; set; }
        public long TotalRowsProcessed { get; set; }
        public double ProgressPercentage { get; set; }
        public double CurrentSpeed { get; set; } // rows per second
    }

    /// <summary>
    /// Event arguments for status changed event
    /// </summary>
    public class StatusEventArgs : EventArgs
    {
        public ProcessingStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Interface for reporting processing progress to UI
    /// </summary>
    public interface IProgressReporter
    {
        /// <summary>
        /// Event fired when processing progress changes
        /// </summary>
        event EventHandler<ProgressEventArgs>? ProgressChanged;

        /// <summary>
        /// Event fired when processing status changes
        /// </summary>
        event EventHandler<StatusEventArgs>? StatusChanged;

        /// <summary>
        /// Reports progress update
        /// </summary>
        void ReportProgress(int fileIndex, int totalFiles, string fileName, long rowsProcessed, double speed);

        /// <summary>
        /// Reports status change
        /// </summary>
        void ReportStatus(ProcessingStatus status, string message);
    }
}
