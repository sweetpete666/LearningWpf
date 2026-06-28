using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Configuration;
using System.Data;
using System.Windows;
using LearningWpf.ViewModels;

namespace LearningWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            // 1. Serilog als globalen Logger konfigurieren
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug() // Ausgabe im VS-Debug-Fenster
                .WriteTo.File("logs/app-log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // 2. Den .NET Generic Host aufbauen (DI-Container + Logging)
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders(); // Standard-Konsolen-Logger entfernen
                    logging.AddSerilog();     // Serilog an das Microsoft-Logging anbinden
                })
                .ConfigureServices((context, services) =>
                {
                    // 3. Hier registrieren Sie Ihre Fenster und ViewModels für DI
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<MainWindowViewModel>(); // Falls Sie MVVM nutzen
                })
                .Build();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Host beim Anwendungsstart hochfahren
            AppHost?.Start();

            // Das MainWindow sicher über den DI-Container anfordern und anzeigen
            var mainWindow = AppHost?.Services.GetRequiredService<MainWindow>();
            mainWindow?.Show();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Host sauber stoppen und Ressourcen freigeben
            AppHost?.StopAsync().GetAwaiter().GetResult();
            Log.CloseAndFlush(); // Serilog-Buffer leeren

            base.OnExit(e);
        }

    }

}
