using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BulkInApp.Core.Configuration;
using BulkInApp.Core.Utilities;
using Microsoft.Data.SqlClient;

namespace BulkIn.Desktop.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly SqlConnectionFactory _connectionFactory;
        private readonly string _configFilePath;

        // Database Settings
        [ObservableProperty]
        private string _serverName = "localhost";

        [ObservableProperty]
        private string _databaseName = "TextFileDB";

        [ObservableProperty]
        private bool _useTrustedConnection = true;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private int _connectionTimeout = 30;

        [ObservableProperty]
        private string? _connectionStatusMessage;

        [ObservableProperty]
        private string _connectionStatusColor = "#6B7280";

        // File Processing Settings
        [ObservableProperty]
        private string _sourceFilePath = string.Empty;

        [ObservableProperty]
        private string _filePatterns = "*.txt";

        [ObservableProperty]
        private int _batchSize = 10000;

        // Logging Settings
        [ObservableProperty]
        private string _logFilePath = string.Empty;

        [ObservableProperty]
        private bool _enableConsoleLogging = true;

        [ObservableProperty]
        private string _logLevel = "Information";

        // Save Status
        [ObservableProperty]
        private string? _saveStatusMessage;

        [ObservableProperty]
        private string _saveStatusColor = "#6B7280";

        public SettingsViewModel()
        {
            // Default constructor for design-time
            _connectionFactory = null!;
            _configFilePath = "appsettings.json";
        }

        public SettingsViewModel(SqlConnectionFactory connectionFactory, string configFilePath = "appsettings.json")
        {
            _connectionFactory = connectionFactory;
            _configFilePath = configFilePath;
            LoadSettings();
        }

        /// <summary>
        /// Loads settings from appsettings.json
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                var configLoader = new ConfigurationLoader(_configFilePath);
                var appSettings = configLoader.LoadConfiguration();

                // Load Database Settings
                ServerName = appSettings.DatabaseSettings.ServerName;
                DatabaseName = appSettings.DatabaseSettings.DatabaseName;
                UseTrustedConnection = appSettings.DatabaseSettings.UseTrustedConnection;
                Username = appSettings.DatabaseSettings.Username ?? string.Empty;
                Password = appSettings.DatabaseSettings.Password ?? string.Empty;
                ConnectionTimeout = appSettings.DatabaseSettings.ConnectionTimeout;

                // Load File Settings
                SourceFilePath = appSettings.FileSettings.SourceFilePath;
                FilePatterns = string.Join(", ", appSettings.FileSettings.FilePatterns);

                // Load Processing Settings
                BatchSize = appSettings.ProcessingSettings.BatchSize;

                // Load Logging Settings
                LogFilePath = appSettings.LoggingSettings.LogFilePath;
                EnableConsoleLogging = appSettings.LoggingSettings.EnableConsoleLogging;
                LogLevel = appSettings.LoggingSettings.LogLevel;

                ClearStatusMessages();
            }
            catch (System.Exception ex)
            {
                ShowSaveError($"Failed to load settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves current settings to appsettings.json
        /// </summary>
        [RelayCommand]
        private async Task SaveSettings()
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(ServerName))
                {
                    ShowSaveError("Server Name is required");
                    return;
                }

                if (string.IsNullOrWhiteSpace(DatabaseName))
                {
                    ShowSaveError("Database Name is required");
                    return;
                }

                if (string.IsNullOrWhiteSpace(SourceFilePath))
                {
                    ShowSaveError("Source File Path is required");
                    return;
                }

                if (string.IsNullOrWhiteSpace(LogFilePath))
                {
                    ShowSaveError("Log File Path is required");
                    return;
                }

                // Create updated settings object
                var appSettings = new AppSettings
                {
                    DatabaseSettings = new DatabaseSettings
                    {
                        ServerName = ServerName,
                        DatabaseName = DatabaseName,
                        UseTrustedConnection = UseTrustedConnection,
                        Username = UseTrustedConnection ? null : (string.IsNullOrEmpty(Username) ? null : Username)!,
                        Password = UseTrustedConnection ? null : (string.IsNullOrEmpty(Password) ? null : Password)!,
                        ConnectionTimeout = ConnectionTimeout
                    },
                    FileSettings = new FileSettings
                    {
                        SourceFilePath = SourceFilePath,
                        FilePatterns = FilePatterns.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries).ToList()
                    },
                    ProcessingSettings = new ProcessingSettings
                    {
                        BatchSize = BatchSize
                    },
                    LoggingSettings = new LoggingSettings
                    {
                        LogFilePath = LogFilePath,
                        EnableConsoleLogging = EnableConsoleLogging,
                        LogLevel = LogLevel
                    }
                };

                // Save to file
                var configLoader = new ConfigurationLoader(_configFilePath);
                configLoader.SaveConfiguration(appSettings);

                ShowSaveSuccess("Settings saved successfully!");

                // Auto-clear success message after 3 seconds
                await Task.Delay(3000);
                if (SaveStatusMessage == "Settings saved successfully!")
                {
                    SaveStatusMessage = null;
                }
            }
            catch (System.Exception ex)
            {
                ShowSaveError($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests database connection with current settings
        /// </summary>
        [RelayCommand]
        private async Task TestConnection()
        {
            try
            {
                ConnectionStatusMessage = "Testing connection...";
                ConnectionStatusColor = "#F59E0B"; // Orange for in-progress

                await Task.Run(() =>
                {
                    var connectionString = BuildConnectionString();
                    using var connection = new SqlConnection(connectionString);
                    connection.Open();
                    connection.Close();
                });

                ShowConnectionSuccess("✓ Connection successful!");

                // Auto-clear success message after 5 seconds
                await Task.Delay(5000);
                if (ConnectionStatusMessage == "✓ Connection successful!")
                {
                    ConnectionStatusMessage = null;
                }
            }
            catch (System.Exception ex)
            {
                ShowConnectionError($"✗ Connection failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Resets all settings to default values
        /// </summary>
        [RelayCommand]
        private void ResetSettings()
        {
            ServerName = "localhost";
            DatabaseName = "TextFileDB";
            UseTrustedConnection = true;
            Username = string.Empty;
            Password = string.Empty;
            ConnectionTimeout = 30;
            SourceFilePath = string.Empty;
            FilePatterns = "*.txt";
            BatchSize = 10000;
            LogFilePath = string.Empty;
            EnableConsoleLogging = true;
            LogLevel = "Information";

            ShowSaveSuccess("Settings reset to defaults");
        }

        /// <summary>
        /// Opens folder browser for source file path
        /// </summary>
        [RelayCommand]
        private async Task BrowseFolder()
        {
            // TODO: Implement folder browser dialog
            // This will be implemented when we add Avalonia.Dialogs or platform-specific dialogs
            await Task.CompletedTask;
        }

        /// <summary>
        /// Opens folder browser for log file path
        /// </summary>
        [RelayCommand]
        private async Task BrowseLogFolder()
        {
            // TODO: Implement folder browser dialog
            await Task.CompletedTask;
        }

        private string BuildConnectionString()
        {
            if (UseTrustedConnection)
            {
                return $"Server={ServerName};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout={ConnectionTimeout}";
            }
            else
            {
                return $"Server={ServerName};Database={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;Connection Timeout={ConnectionTimeout}";
            }
        }

        private void ShowConnectionSuccess(string message)
        {
            ConnectionStatusMessage = message;
            ConnectionStatusColor = "#10B981"; // Green
        }

        private void ShowConnectionError(string message)
        {
            ConnectionStatusMessage = message;
            ConnectionStatusColor = "#EF4444"; // Red
        }

        private void ShowSaveSuccess(string message)
        {
            SaveStatusMessage = message;
            SaveStatusColor = "#10B981"; // Green
        }

        private void ShowSaveError(string message)
        {
            SaveStatusMessage = message;
            SaveStatusColor = "#EF4444"; // Red
        }

        private void ClearStatusMessages()
        {
            ConnectionStatusMessage = null;
            SaveStatusMessage = null;
        }
    }
}
