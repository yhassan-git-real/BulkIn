using System;
using System.Collections.Generic;
using BulkInApp.Core.Services.Interfaces;
using BulkInApp.Core.Models;
using CoreLogLevel = BulkInApp.Core.Services.Interfaces.LogLevel;
using UILogLevel = BulkIn.Desktop.Models.LogLevel;

namespace BulkIn.Desktop.Services
{
    /// <summary>
    /// UI-aware logging service that wraps the Core logging service
    /// and provides simplified events for the UI to consume
    /// </summary>
    public class UILoggingService
    {
        private readonly ILoggingService _coreLoggingService;

        public event EventHandler<LogEventArgs>? LogAdded;

        public UILoggingService(ILoggingService coreLoggingService)
        {
            _coreLoggingService = coreLoggingService ?? throw new ArgumentNullException(nameof(coreLoggingService));
            
            // Subscribe to Core logging events and convert them to UI events
            _coreLoggingService.LogEntryAdded += OnCoreLogEntryAdded;
        }

        private void OnCoreLogEntryAdded(object? sender, LogEntryEventArgs e)
        {
            // Convert Core LogLevel to UI LogLevel
            var uiLevel = ConvertLogLevel(e.Level);
            var message = e.Exception != null ? $"{e.Message}: {e.Exception.Message}" : e.Message;
            
            LogAdded?.Invoke(this, new LogEventArgs(uiLevel, message));
        }

        private UILogLevel ConvertLogLevel(CoreLogLevel coreLevel)
        {
            return coreLevel switch
            {
                CoreLogLevel.Information => UILogLevel.Info,
                CoreLogLevel.Warning => UILogLevel.Warning,
                CoreLogLevel.Error => UILogLevel.Error,
                CoreLogLevel.Fatal => UILogLevel.Error,
                CoreLogLevel.Debug => UILogLevel.Debug,
                CoreLogLevel.Verbose => UILogLevel.Debug,
                _ => UILogLevel.Info
            };
        }

        // Convenience methods for direct logging (these delegate to Core service)
        public void LogInfo(string message) => _coreLoggingService.LogInfo(message);
        public void LogWarning(string message) => _coreLoggingService.LogWarning(message);
        public void LogError(string message, Exception? exception = null) => _coreLoggingService.LogError(message, exception);
    }

    /// <summary>
    /// Event arguments for log events
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        public UILogLevel Level { get; }
        public string Message { get; }
        public DateTime Timestamp { get; }

        public LogEventArgs(UILogLevel level, string message)
        {
            Level = level;
            Message = message;
            Timestamp = DateTime.Now;
        }
    }
}
