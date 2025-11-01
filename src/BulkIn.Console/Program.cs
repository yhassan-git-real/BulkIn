using System;
using BulkInApp.Configuration;
using BulkInApp.Services;
using BulkInApp.Utilities;
using Console = System.Console;

namespace BulkInApp
{
    /// <summary>
    /// BulkIn - Bulk Text File Data Ingestion System
    /// Main entry point for the application
    /// </summary>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Set console encoding to UTF-8 for emoji support
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            LoggingService? loggingService = null;

            try
            {
                // Display application banner
                DisplayBanner();

                // Step 1: Load Configuration
                Console.WriteLine("⚙️  Loading configuration...");
                var configLoader = new ConfigurationLoader("appsettings.json");
                var appSettings = configLoader.LoadConfiguration();
                Console.WriteLine("✅ Configuration loaded successfully");
                Console.WriteLine();

                // Display configuration summary
                ConfigurationLoader.DisplayConfigurationSummary(appSettings);
                Console.WriteLine();

                // Step 2: Initialize Logging Service
                loggingService = new LoggingService(appSettings.LoggingSettings);
                loggingService.LogInfo("BulkIn application started");

                // Step 3: Initialize Services
                var connectionFactory = new SqlConnectionFactory(appSettings.DatabaseSettings);
                var fileHelper = new FileHelper(appSettings.FileSettings);
                var fileReaderService = new FileReaderService(appSettings.ProcessingSettings);
                var bulkInsertService = new BulkInsertService(
                    appSettings.DatabaseSettings,
                    appSettings.ProcessingSettings,
                    connectionFactory);
                var dataTransferService = new DataTransferService(
                    appSettings.DatabaseSettings,
                    connectionFactory);
                var fileProcessorService = new FileProcessorService(
                    appSettings,
                    fileReaderService,
                    bulkInsertService,
                    dataTransferService,
                    connectionFactory,
                    fileHelper,
                    loggingService);

                // Step 4: Validate Prerequisites
                PrintSectionHeader("🔍 VALIDATION");
                Console.Write($"   {ConsoleColors.Info("Testing database connection...")}");
                
                if (connectionFactory.TestConnection(out string dbError))
                {
                    Console.WriteLine($" {ConsoleColors.Success("✅")}");
                    loggingService.LogDatabaseConnectionTest(true);
                }
                else
                {
                    Console.WriteLine($" {ConsoleColors.Error("❌")}");
                    Console.WriteLine($"   {ConsoleColors.Error("Error:")} {dbError}");
                    loggingService.LogDatabaseConnectionTest(false, dbError);
                    return 1;
                }

                if (!fileProcessorService.ValidatePrerequisites(out List<string> validationErrors))
                {
                    Console.WriteLine($"   {ConsoleColors.Error("❌ Validation failed:")}");
                    foreach (var error in validationErrors)
                    {
                        Console.WriteLine($"      {ConsoleColors.Secondary("•")} {error}");
                        loggingService.LogError(error);
                    }
                    loggingService.LogConfigurationValidation(false, validationErrors);
                    return 1;
                }

                Console.WriteLine($"   {ConsoleColors.Success("✅ All prerequisites validated")}");
                loggingService.LogConfigurationValidation(true, new List<string>());
                Console.WriteLine();

                // Step 5: Discover Files
                PrintSectionHeader("📁 FILE DISCOVERY");
                loggingService.LogInfo("Discovering files to process...");
                
                var files = fileHelper.DiscoverFiles();
                
                if (files.Count == 0)
                {
                    Console.WriteLine($"{ConsoleColors.Warning("⚠️  No files found matching the specified criteria.")}");
                    loggingService.LogWarning("No files found to process");
                    return 0;
                }

                Console.WriteLine($"{ConsoleColors.Success($"✅ Found {files.Count} file(s) to process")}");
                Console.WriteLine();

                // Display file list preview
                if (files.Count <= 10)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        Console.WriteLine($"   [{i + 1}] {Path.GetFileName(files[i])}");
                    }
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Console.WriteLine($"   [{i + 1}] {Path.GetFileName(files[i])}");
                    }
                    Console.WriteLine($"   ⋮");
                    for (int i = files.Count - 3; i < files.Count; i++)
                    {
                        Console.WriteLine($"   [{i + 1}] {Path.GetFileName(files[i])}");
                    }
                }
                Console.WriteLine();

                // Step 6: Confirm Processing
                PrintSeparator();
                Console.Write("⏸️  Press ENTER to start processing (or Ctrl+C to cancel): ");
                Console.ReadLine();
                Console.WriteLine();

                // Step 7: Process Files
                PrintSectionHeader("🚀 BATCH PROCESSING");

                var stats = await fileProcessorService.ProcessFilesAsync(files);

                // Step 8: Display Final Summary (already logged by LoggingService)
                Console.WriteLine();

                // Determine exit code based on results
                if (stats.FilesFailed > 0)
                {
                    loggingService.LogWarning($"Batch completed with {stats.FilesFailed} failed file(s)");
                    return 2; // Partial success
                }

                loggingService.LogInfo("Batch processing completed successfully");
                return 0; // Success
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine();
                Console.WriteLine("❌ Configuration Error");
                Console.WriteLine($"   {ex.Message}");
                Console.WriteLine("   Please ensure appsettings.json exists in the application directory.");
                loggingService?.LogError("Configuration file not found", ex);
                return 1;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine();
                Console.WriteLine("❌ Configuration Error");
                Console.WriteLine($"   {ex.Message}");
                loggingService?.LogError("Configuration validation failed", ex);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("❌ Unexpected Error");
                Console.WriteLine($"   {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("   Stack Trace:");
                Console.WriteLine($"   {ex.StackTrace}");
                loggingService?.LogError("Unexpected error occurred", ex);
                return 1;
            }
            finally
            {
                // Cleanup and dispose logging service
                loggingService?.Dispose();
                
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Displays the application banner
        /// </summary>
        static void DisplayBanner()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    BulkIn v1.0                             ║");
            Console.WriteLine("║         Bulk Text File Data Ingestion System               ║");
            Console.WriteLine("║                                                            ║");
            Console.WriteLine("║  Purpose: Load large text files into SQL Server            ║");
            Console.WriteLine("║  Tech Stack: .NET 8, C#, SQL Server                        ║");
            Console.WriteLine("║  Date: October 31, 2025                                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        /// <summary>
        /// Prints a section header with emoji and title
        /// </summary>
        static void PrintSectionHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine($" {title}");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
        }

        /// <summary>
        /// Prints a separator line
        /// </summary>
        static void PrintSeparator()
        {
            Console.WriteLine("───────────────────────────────────────────────────────────");
        }
    }
}
