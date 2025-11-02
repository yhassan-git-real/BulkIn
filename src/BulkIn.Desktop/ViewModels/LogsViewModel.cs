using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BulkIn.Desktop.Models;
using BulkIn.Desktop.Services;

namespace BulkIn.Desktop.ViewModels
{
    public partial class LogsViewModel : ViewModelBase
    {
        private const int MAX_LOG_ENTRIES = 5000; // Limit log entries to prevent memory issues
        private readonly UILoggingService? _loggingService;

        [ObservableProperty]
        private ObservableCollection<LogEntry> _allLogs = new();

        [ObservableProperty]
        private ObservableCollection<LogEntry> _filteredLogs = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showInfo = true;

        [ObservableProperty]
        private bool _showWarning = true;

        [ObservableProperty]
        private bool _showError = true;

        [ObservableProperty]
        private bool _autoScroll = true;

        public LogsViewModel(UILoggingService? loggingService = null)
        {
            _loggingService = loggingService;

            // Subscribe to property changes for filtering
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SearchText) ||
                    e.PropertyName == nameof(ShowInfo) ||
                    e.PropertyName == nameof(ShowWarning) ||
                    e.PropertyName == nameof(ShowError))
                {
                    ApplyFilters();
                }
            };

            // Subscribe to logging service events
            if (_loggingService != null)
            {
                _loggingService.LogAdded += OnLogAdded;
            }

            // Add welcome message
            AddLog(LogLevel.Info, "Log viewer initialized. Monitoring application events...");
        }

        private void OnLogAdded(object? sender, LogEventArgs e)
        {
            // Ensure we're on the UI thread
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                AddLog(e.Level, e.Message);
            });
        }

        /// <summary>
        /// Adds a new log entry
        /// </summary>
        public void AddLog(LogLevel level, string message)
        {
            var logEntry = new LogEntry(level, message);
            
            // Add to all logs
            AllLogs.Add(logEntry);

            // Limit log entries
            if (AllLogs.Count > MAX_LOG_ENTRIES)
            {
                AllLogs.RemoveAt(0);
            }

            // Apply filters to update filtered logs
            ApplyFilters();
        }

        /// <summary>
        /// Applies current filters to log list
        /// </summary>
        private void ApplyFilters()
        {
            var filtered = AllLogs.AsEnumerable();

            // Filter by level
            filtered = filtered.Where(log =>
                (ShowInfo && log.Level == LogLevel.Info) ||
                (ShowWarning && log.Level == LogLevel.Warning) ||
                (ShowError && log.Level == LogLevel.Error) ||
                (log.Level == LogLevel.Success) || // Always show success
                (log.Level == LogLevel.Debug)       // Always show debug
            );

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(log =>
                    log.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            FilteredLogs = new ObservableCollection<LogEntry>(filtered);
        }

        /// <summary>
        /// Clears all logs
        /// </summary>
        [RelayCommand]
        private void ClearLogs()
        {
            AllLogs.Clear();
            FilteredLogs.Clear();
            AddLog(LogLevel.Info, "Logs cleared");
        }

        /// <summary>
        /// Exports logs to a text file
        /// </summary>
        [RelayCommand]
        private async Task ExportLogs()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"BulkIn_Logs_{timestamp}.txt";
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var filePath = Path.Combine(desktopPath, fileName);

                var lines = FilteredLogs.Select(log =>
                    $"[{log.FormattedTimestamp}] [{log.Level}] {log.Message}");

                await File.WriteAllLinesAsync(filePath, lines);

                AddLog(LogLevel.Success, $"Logs exported to: {filePath}");
            }
            catch (Exception ex)
            {
                AddLog(LogLevel.Error, $"Failed to export logs: {ex.Message}");
            }
        }
    }
}
