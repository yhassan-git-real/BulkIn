using System;
using BulkInApp.Core.Services.Interfaces;

namespace BulkIn.Desktop.Services
{
    /// <summary>
    /// Progress reporter implementation for UI integration
    /// </summary>
    public class UIProgressReporter : IProgressReporter
    {
        public event EventHandler<ProgressEventArgs>? ProgressChanged;
        public event EventHandler<StatusEventArgs>? StatusChanged;

        public void ReportProgress(int fileIndex, int totalFiles, string fileName, long rowsProcessed, double speed)
        {
            ProgressChanged?.Invoke(this, new ProgressEventArgs
            {
                CurrentFileIndex = fileIndex,
                TotalFiles = totalFiles,
                CurrentFileName = fileName,
                RowsProcessed = rowsProcessed,
                ProgressPercentage = totalFiles > 0 ? (fileIndex * 100.0 / totalFiles) : 0,
                CurrentSpeed = speed
            });
        }

        public void ReportStatus(ProcessingStatus status, string message)
        {
            StatusChanged?.Invoke(this, new StatusEventArgs
            {
                Status = status,
                Message = message
            });
        }
    }
}
