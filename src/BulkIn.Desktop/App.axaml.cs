using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using BulkIn.Desktop.ViewModels;
using BulkIn.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using BulkInApp.Core.Configuration;
using BulkInApp.Core.Services;
using BulkInApp.Core.Services.Interfaces;
using BulkInApp.Core.Utilities;
using System;

namespace BulkIn.Desktop;

public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // Set up Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();

            // Create main window with DI-resolved ViewModel
            var mainViewModel = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Load configuration from appsettings.json
        var configLoader = new ConfigurationLoader("appsettings.json");
        var appSettings = configLoader.LoadConfiguration();
        
        // Register configuration objects
        services.AddSingleton(appSettings);
        services.AddSingleton(appSettings.DatabaseSettings);
        services.AddSingleton(appSettings.FileSettings);
        services.AddSingleton(appSettings.ProcessingSettings);
        services.AddSingleton(appSettings.LoggingSettings);

        // Register Core services
        services.AddSingleton<SqlConnectionFactory>();
        services.AddSingleton<FileHelper>();
        services.AddSingleton<FileReaderService>();
        services.AddSingleton<BulkInsertService>();
        services.AddSingleton<DataTransferService>();
        
        // Register services with interfaces
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IFileProcessor, FileProcessorService>();
        
        // Register Desktop-specific services
        services.AddSingleton<BulkIn.Desktop.Services.UILoggingService>();
        services.AddSingleton<BulkIn.Desktop.Services.UIProgressReporter>();

        // Register ViewModels
        services.AddSingleton<LogsViewModel>(); // Singleton to maintain log history
        services.AddTransient<SettingsViewModel>(sp => 
            new SettingsViewModel(sp.GetRequiredService<SqlConnectionFactory>(), "appsettings.json"));
        services.AddTransient<ProcessingViewModel>();
        services.AddTransient<MainWindowViewModel>();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}