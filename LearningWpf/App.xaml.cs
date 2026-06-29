using Company.Shared.Bootstrapping;
using LearningWpf.Repositories;
using LearningWpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

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
            var hostBuilder = AppBootstrapper.Instance
                .AddJsonFiles(
                    GlobalLibrary.Instance.GetPaths("ApiSettings", "DatabaseConfig")
                )
                .CreateHostBuilder();

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

        protected override async void OnExit(ExitEventArgs e)
        {
            if (AppHost != null)
            {
                await AppHost.StopAsync();
                AppHost.Dispose();
            }
            base.OnExit(e);
        }

    }

}
