using CommunityToolkit.Mvvm.ComponentModel;

namespace BulkIn.Desktop.Models
{
    /// <summary>
    /// Represents a file in the processing queue with status tracking
    /// </summary>
    public partial class FileItem : ObservableObject
    {
        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _fullPath = string.Empty;

        [ObservableProperty]
        private FileStatus _status = FileStatus.Pending;

        [ObservableProperty]
        private string _statusIcon = "⏳";

        [ObservableProperty]
        private string _statusColor = "#9CA3AF";

        [ObservableProperty]
        private long _recordsProcessed = 0;

        [ObservableProperty]
        private string? _errorMessage;

        [ObservableProperty]
        private double _fileProgress = 0;

        /// <summary>
        /// Updates status and corresponding icon/color
        /// </summary>
        public void UpdateStatus(FileStatus newStatus)
        {
            Status = newStatus;
            
            (StatusIcon, StatusColor) = newStatus switch
            {
                FileStatus.Pending => ("⏳", "#9CA3AF"),
                FileStatus.Processing => ("⚙️", "#0078D4"),
                FileStatus.Success => ("✅", "#10B981"),
                FileStatus.Failed => ("❌", "#EF4444"),
                FileStatus.Skipped => ("⏭️", "#F59E0B"),
                _ => ("❓", "#6B7280")
            };
        }
    }

    /// <summary>
    /// File processing status
    /// </summary>
    public enum FileStatus
    {
        Pending,
        Processing,
        Success,
        Failed,
        Skipped
    }
}
