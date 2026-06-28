using LearningWpf.Helper;
using LearningWpf.Repositories;
using LearningWpf.ViewModels;
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
            // 1. Konsole initialisieren (lädt Position aus Registry)
            this.consoleManager.InitializeConsole();

            // 2. Den fertig konfigurierten Builder über den Manager anfordern
            var hostBuilder = ConfigurationManager.CreateHostBuilder();

            // 3. Nur noch die reinen Anwendungs-Services registrieren und bauen
            AppHost = hostBuilder
                .ConfigureServices((context, services) =>
                {
                    services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));
                    services.AddSingleton<IUserRepository, MockUserRepository>();

                    services.AddSingleton<MainWindow>();
                    services.AddTransient<MainWindowViewModel>();
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
