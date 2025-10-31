using Microsoft.Extensions.Configuration;

namespace BulkIn.Configuration
{
    /// <summary>
    /// Loads and validates application configuration from appsettings.json
    /// </summary>
    public class ConfigurationLoader
    {
        private readonly string _configFilePath;

        /// <summary>
        /// Initializes a new instance of ConfigurationLoader
        /// </summary>
        /// <param name="configFilePath">Path to appsettings.json (default: current directory)</param>
        public ConfigurationLoader(string configFilePath = "appsettings.json")
        {
            _configFilePath = configFilePath;
        }

        /// <summary>
        /// Loads the application settings from appsettings.json
        /// </summary>
        /// <returns>Populated AppSettings object</returns>
        /// <exception cref="FileNotFoundException">Thrown when appsettings.json is not found</exception>
        /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
        public AppSettings LoadConfiguration()
        {
            // Check if configuration file exists
            if (!File.Exists(_configFilePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {_configFilePath}");
            }

            try
            {
                // Build configuration from JSON file
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(_configFilePath, optional: false, reloadOnChange: false)
                    .Build();

                // Create and populate AppSettings
                var appSettings = new AppSettings();
                configuration.Bind(appSettings);

                // Validate configuration
                if (!appSettings.IsValid(out List<string> errorMessages))
                {
                    var errorDetails = string.Join(Environment.NewLine, errorMessages);
                    throw new InvalidOperationException(
                        $"Configuration validation failed:{Environment.NewLine}{errorDetails}");
                }

                // Ensure log directory exists
                EnsureLogDirectoryExists(appSettings.LoggingSettings.LogFilePath);

                return appSettings;
            }
            catch (Exception ex) when (ex is not FileNotFoundException && ex is not InvalidOperationException)
            {
                throw new InvalidOperationException(
                    $"Failed to load configuration from {_configFilePath}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates the log directory if it doesn't exist
        /// </summary>
        private void EnsureLogDirectoryExists(string logPath)
        {
            if (!Directory.Exists(logPath))
            {
                try
                {
                    Directory.CreateDirectory(logPath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Failed to create log directory: {logPath}. Error: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Displays configuration summary to console
        /// </summary>
        public static void DisplayConfigurationSummary(AppSettings settings)
        {
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("               CONFIGURATION SUMMARY                        ");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();
            
            Console.WriteLine("📊 Database:");
            Console.WriteLine($"   {settings.DatabaseSettings.ServerName}\\{settings.DatabaseSettings.DatabaseName} → {settings.DatabaseSettings.TargetTableName}");
            Console.WriteLine($"   Auth: {(settings.DatabaseSettings.UseTrustedConnection ? "Windows" : "SQL")} • Temp: {settings.DatabaseSettings.TempTableName}");
            Console.WriteLine();

            Console.WriteLine("📁 Files:");
            Console.WriteLine($"   Path: {settings.FileSettings.SourceFilePath}");
            
            var effectivePatterns = settings.FileSettings.GetEffectivePatterns();
            var patternsDisplay = effectivePatterns.Count > 1 
                ? string.Join(", ", effectivePatterns) 
                : effectivePatterns.First();
            
            Console.WriteLine($"   Pattern: {patternsDisplay} • Sort: {(settings.FileSettings.ProcessInAlphabeticalOrder ? "A-Z" : "Discovery")}");
            Console.WriteLine();

            Console.WriteLine("⚙️  Processing:");
            Console.WriteLine($"   Batch: {settings.ProcessingSettings.BatchSize:N0} rows • Transaction: {(settings.ProcessingSettings.EnableTransactionPerFile ? "Yes" : "No")} • Continue on Error: {(settings.ProcessingSettings.ContinueOnError ? "Yes" : "No")}");
            Console.WriteLine();

            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine();
        }
    }
}
