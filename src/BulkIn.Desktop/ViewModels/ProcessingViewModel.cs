using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BulkInApp.Core.Configuration;
using BulkInApp.Core.Services.Interfaces;
using BulkInApp.Core.Models;
using BulkInApp.Core.Utilities;
using BulkIn.Desktop.Models;
using BulkIn.Desktop.Services;

namespace BulkIn.Desktop.ViewModels
{
    public partial class ProcessingViewModel : ViewModelBase
    {
        private readonly IFileProcessor _fileProcessor;
        private readonly FileHelper _fileHelper;
        private readonly FileSettings _fileSettings;
        private readonly UIProgressReporter _progressReporter;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly Stopwatch _stopwatch = new();
        private bool _isPauseRequested = false;
        private DateTime _lastUIUpdate = DateTime.MinValue;
        private const int UI_UPDATE_THROTTLE_MS = 100; // Update UI max once per 100ms

        // File List
        [ObservableProperty]
        private ObservableCollection<FileItem> _fileList = new();

        [ObservableProperty]
        private string _sourceFilePath = string.Empty;

        [ObservableProperty]
        private string _fileCountText = "0 files found";

        // Processing State
        [ObservableProperty]
        private bool _isProcessing = false;

        [ObservableProperty]
        private bool _isPaused = false;

        [ObservableProperty]
        private bool _canStartProcessing = false;

        [ObservableProperty]
        private bool _canPauseProcessing = false;

        [ObservableProperty]
        private bool _canStopProcessing = false;

        // Progress
        [ObservableProperty]
        private double _progressPercentage = 0;

        [ObservableProperty]
        private string _progressPercentageText = "0%";

        [ObservableProperty]
        private string _progressStatusText = "Ready to process files";

        [ObservableProperty]
        private string? _currentFileProcessing;

        // Statistics
        [ObservableProperty]
        private int _totalFilesCount = 0;

        [ObservableProperty]
        private int _processedFilesCount = 0;

        [ObservableProperty]
        private int _failedFilesCount = 0;

        [ObservableProperty]
        private long _totalRecordsProcessed = 0;

        [ObservableProperty]
        private string _elapsedTime = "00:00:00";

        [ObservableProperty]
        private string _processingRate = "0 records/sec";

        private FileProcessingStats? _currentStats;

        public ProcessingViewModel()
        {
            // Design-time constructor
            _fileProcessor = null!;
            _fileHelper = null!;
            _fileSettings = new FileSettings { SourceFilePath = "C:\\Data\\SourceFiles" };
            _progressReporter = new UIProgressReporter();
            SourceFilePath = _fileSettings.SourceFilePath;
        }

        public ProcessingViewModel(IFileProcessor fileProcessor, FileHelper fileHelper, FileSettings fileSettings)
        {
            _fileProcessor = fileProcessor;
            _fileHelper = fileHelper;
            _fileSettings = fileSettings;
            _progressReporter = new UIProgressReporter();
            
            // Subscribe to progress events
            _progressReporter.ProgressChanged += OnProgressChanged;
            _progressReporter.StatusChanged += OnStatusChanged;
            
            SourceFilePath = _fileSettings.SourceFilePath;
            
            // Load files on initialization
            LoadFiles();
        }

        /// <summary>
        /// Loads files from the source directory
        /// </summary>
        private void LoadFiles()
        {
            try
            {
                FileList.Clear();
                
                if (string.IsNullOrWhiteSpace(_fileSettings.SourceFilePath) || 
                    !Directory.Exists(_fileSettings.SourceFilePath))
                {
                    FileCountText = "Source directory not found";
                    CanStartProcessing = false;
                    return;
                }

                var files = _fileHelper.DiscoverFiles();

                foreach (var file in files)
                {
                    var fileItem = new FileItem
                    {
                        FileName = Path.GetFileName(file),
                        FullPath = file,
                        Status = FileStatus.Pending
                    };
                    fileItem.UpdateStatus(FileStatus.Pending);
                    FileList.Add(fileItem);
                }

                TotalFilesCount = files.Count;
                FileCountText = $"{files.Count} file{(files.Count != 1 ? "s" : "")} found";
                CanStartProcessing = files.Count > 0;
            }
            catch (Exception ex)
            {
                FileCountText = $"Error loading files: {ex.Message}";
                CanStartProcessing = false;
            }
        }

        /// <summary>
        /// Refreshes the file list
        /// </summary>
        [RelayCommand]
        private void RefreshFiles()
        {
            LoadFiles();
            ResetStatistics();
        }

        /// <summary>
        /// Starts file processing
        /// </summary>
        [RelayCommand]
        private async Task StartProcessing()
        {
            try
            {
                // Validate prerequisites
                if (!_fileProcessor.ValidatePrerequisites(out var errors))
                {
                    ProgressStatusText = $"Prerequisites failed: {string.Join(", ", errors)}";
                    return;
                }

                // Reset statistics
                ResetStatistics();

                // Set processing state
                IsProcessing = true;
                IsPaused = false;
                CanStartProcessing = false;
                CanPauseProcessing = true;
                CanStopProcessing = true;
                ProgressStatusText = "Processing files...";

                // Create cancellation token
                _cancellationTokenSource = new CancellationTokenSource();
                _stopwatch.Restart();

                // Get file paths
                var filePaths = _fileHelper.DiscoverFiles();

                // Start progress update timer
                var progressTimer = new System.Timers.Timer(500); // Update every 500ms
                progressTimer.Elapsed += (s, e) => UpdateElapsedTime();
                progressTimer.Start();

                try
                {
                    // Process files one by one with progress updates
                    _currentStats = new FileProcessingStats
                    {
                        TotalFilesDiscovered = filePaths.Count,
                        BatchStartTime = DateTime.Now
                    };

                    for (int i = 0; i < filePaths.Count; i++)
                    {
                        // Check for cancellation
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            ProgressStatusText = "Processing stopped by user";
                            break;
                        }

                        // Check for pause
                        while (_isPauseRequested && !_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            await Task.Delay(100);
                        }

                        var filePath = filePaths[i];
                        var fileName = Path.GetFileName(filePath);
                        CurrentFileProcessing = $"Processing: {fileName}";

                        // Find and update file item status
                        var fileItem = FileList.FirstOrDefault(f => f.FullPath == filePath);
                        if (fileItem != null)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() => 
                                fileItem.UpdateStatus(FileStatus.Processing));
                        }

                        // Update progress
                        ProgressPercentage = (i * 100.0) / filePaths.Count;
                        ProgressPercentageText = $"{ProgressPercentage:F1}%";

                        // Report progress
                        _progressReporter.ReportProgress(i + 1, filePaths.Count, fileName, 0, 0);

                        // Process file
                        var result = await _fileProcessor.ProcessFileAsync(filePath);
                        _currentStats.AddResult(result);

                        // Update file item with result
                        if (fileItem != null)
                        {
                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                fileItem.UpdateStatus(result.Success ? FileStatus.Success : FileStatus.Failed);
                                fileItem.RecordsProcessed = result.RowsProcessed;
                                if (!result.Success)
                                {
                                    fileItem.ErrorMessage = result.ErrorMessage;
                                }
                            });
                        }

                        // Update statistics
                        ProcessedFilesCount = _currentStats.FilesProcessedSuccessfully;
                        FailedFilesCount = _currentStats.FilesFailed;
                        TotalRecordsProcessed = _currentStats.TotalRowsProcessed;
                        
                        UpdateProcessingRate();
                    }

                    _currentStats.BatchEndTime = DateTime.Now;
                    _currentStats.TotalDuration = _currentStats.BatchEndTime - _currentStats.BatchStartTime;

                    // Final progress update
                    ProgressPercentage = 100;
                    ProgressPercentageText = "100%";
                    CurrentFileProcessing = null;
                    
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        ProgressStatusText = $"Stopped - {ProcessedFilesCount}/{TotalFilesCount} files processed";
                    }
                    else
                    {
                        ProgressStatusText = $"Completed - {ProcessedFilesCount}/{TotalFilesCount} files processed successfully";
                    }
                }
                finally
                {
                    progressTimer.Stop();
                    progressTimer.Dispose();
                    _stopwatch.Stop();
                }
            }
            catch (Exception ex)
            {
                ProgressStatusText = $"Error: {ex.Message}";
            }
            finally
            {
                // Reset processing state
                IsProcessing = false;
                IsPaused = false;
                CanStartProcessing = true;
                CanPauseProcessing = false;
                CanStopProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Pauses or resumes file processing
        /// </summary>
        [RelayCommand]
        private void PauseProcessing()
        {
            _isPauseRequested = !_isPauseRequested;
            IsPaused = _isPauseRequested;
            
            if (_isPauseRequested)
            {
                ProgressStatusText = "Processing paused - Click Pause again to resume";
                _stopwatch.Stop();
            }
            else
            {
                ProgressStatusText = "Processing resumed";
                _stopwatch.Start();
            }
        }

        /// <summary>
        /// Stops file processing
        /// </summary>
        [RelayCommand]
        private void StopProcessing()
        {
            _cancellationTokenSource?.Cancel();
            ProgressStatusText = "Stopping processing...";
            CanStopProcessing = false;
        }

        /// <summary>
        /// Resets all statistics to zero
        /// </summary>
        private void ResetStatistics()
        {
            ProcessedFilesCount = 0;
            FailedFilesCount = 0;
            TotalRecordsProcessed = 0;
            ProgressPercentage = 0;
            ProgressPercentageText = "0%";
            ElapsedTime = "00:00:00";
            ProcessingRate = "0 records/sec";
            CurrentFileProcessing = null;
            _currentStats = null;
        }

        /// <summary>
        /// Updates elapsed time display
        /// </summary>
        private void UpdateElapsedTime()
        {
            if (_stopwatch.IsRunning)
            {
                var elapsed = _stopwatch.Elapsed;
                ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            }
        }

        /// <summary>
        /// Updates processing rate display
        /// </summary>
        private void UpdateProcessingRate()
        {
            if (_stopwatch.Elapsed.TotalSeconds > 0)
            {
                var rate = TotalRecordsProcessed / _stopwatch.Elapsed.TotalSeconds;
                ProcessingRate = $"{rate:N0} records/sec";
            }
        }

        /// <summary>
        /// Handles progress changed events from file processor (throttled to reduce UI load)
        /// </summary>
        private void OnProgressChanged(object? sender, ProgressEventArgs e)
        {
            // Throttle UI updates to prevent overwhelming the UI thread
            var now = DateTime.Now;
            if ((now - _lastUIUpdate).TotalMilliseconds < UI_UPDATE_THROTTLE_MS)
            {
                return; // Skip this update
            }
            _lastUIUpdate = now;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentFileProcessing = $"Processing: {e.CurrentFileName}";
                ProgressPercentage = e.ProgressPercentage;
                ProgressPercentageText = $"{e.ProgressPercentage:F1}%";
                
                if (e.CurrentSpeed > 0)
                {
                    ProcessingRate = $"{e.CurrentSpeed:N0} records/sec";
                }
            });
        }

        /// <summary>
        /// Handles status changed events from file processor
        /// </summary>
        private void OnStatusChanged(object? sender, StatusEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressStatusText = e.Message;
            });
        }
    }
}
