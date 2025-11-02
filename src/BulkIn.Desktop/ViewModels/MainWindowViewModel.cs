using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BulkIn.Desktop.ViewModels;

/// <summary>
/// Main window ViewModel with tab navigation
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public ProcessingViewModel ProcessingViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }
    public LogsViewModel LogsViewModel { get; }

    public MainWindowViewModel(ProcessingViewModel processingViewModel, SettingsViewModel settingsViewModel, LogsViewModel logsViewModel)
    {
        ProcessingViewModel = processingViewModel;
        SettingsViewModel = settingsViewModel;
        LogsViewModel = logsViewModel;
    }

    /// <summary>
    /// Navigate to Processing tab (Ctrl+1)
    /// </summary>
    [RelayCommand]
    private void NavigateToProcessing()
    {
        SelectedTabIndex = 0;
        StatusMessage = "Processing View";
    }

    /// <summary>
    /// Navigate to Settings tab (Ctrl+2)
    /// </summary>
    [RelayCommand]
    private void NavigateToSettings()
    {
        SelectedTabIndex = 1;
        StatusMessage = "Settings View";
    }

    /// <summary>
    /// Navigate to Logs tab (Ctrl+3)
    /// </summary>
    [RelayCommand]
    private void NavigateToLogs()
    {
        SelectedTabIndex = 2;
        StatusMessage = "Logs View";
    }

    /// <summary>
    /// Show About dialog
    /// </summary>
    [RelayCommand]
    private async Task ShowAbout()
    {
        var aboutDialog = new Views.AboutDialog();
        
        // Get the main window
        if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow != null)
        {
            await aboutDialog.ShowDialog(desktop.MainWindow);
        }
    }
}
