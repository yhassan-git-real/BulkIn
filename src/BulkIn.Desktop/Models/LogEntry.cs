using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BulkIn.Desktop.Models
{
    /// <summary>
    /// Represents a log entry for the UI log viewer
    /// </summary>
    public partial class LogEntry : ObservableObject
    {
        [ObservableProperty]
        private DateTime _timestamp;

        [ObservableProperty]
        private LogLevel _level;

        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private string _icon = string.Empty;

        [ObservableProperty]
        private string _color = string.Empty;

        public LogEntry(LogLevel level, string message)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Message = message;
            UpdateIconAndColor();
        }

        private void UpdateIconAndColor()
        {
            (Icon, Color) = Level switch
            {
                LogLevel.Info => ("ℹ️", "#0078D4"),
                LogLevel.Warning => ("⚠️", "#F59E0B"),
                LogLevel.Error => ("❌", "#EF4444"),
                LogLevel.Success => ("✅", "#10B981"),
                LogLevel.Debug => ("🔍", "#6B7280"),
                _ => ("•", "#9CA3AF")
            };
        }

        public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss.fff");
    }

    /// <summary>
    /// Log levels for the UI
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success,
        Debug
    }
}
