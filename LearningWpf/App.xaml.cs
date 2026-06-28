using LearningWpf.Helper;
using LearningWpf.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;

namespace LearningWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ConsoleManager consoleManager = new();
        public static IHost? AppHost { get; private set; }

        public App()
        {
            this.consoleManager.InitializeConsole();

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Die separate Logging-Datei explizit zur Konfiguration hinzufügen
                    config.AddJsonFile("logging.json", optional: false, reloadOnChange: true);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders(); // Standard-Konsolen-Logger entfernen
                                              // Liest nun automatisch aus der serilog.json, da sie Teil der Configuration ist
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(context.Configuration)
                        .CreateLogger();

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

            this.consoleManager.TerminateConsole();

            base.OnExit(e);
        }

    }

}
