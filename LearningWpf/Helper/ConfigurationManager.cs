using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace LearningWpf.Helper
{
    public static class ConfigurationManager
    {
        public static IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder();
            var environment = DetectEnvironment();

            return builder
                .UseEnvironment(environment)
                // 1. SCHRITT: Alle JSON-Dateien einlesen
                .ConfigureAppConfiguration(ConfigureJsonFiles)

                // 2. SCHRITT: Infrastruktur-Services ZUERST registrieren (Wichtig für das Konsolen-Timing!)
                .ConfigureServices((context, services) =>
                {
                    services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));

                    services.AddSingleton<ConsoleManager>();
                    services.AddHostedService<ConsoleStarter>();
                    services.AddHostedService<LifetimeLoggingService>();
                })

                // 3. SCHRITT: Serilog an den Host binden (Jetzt existieren die Konsolen-Dienste bereits im Kontext!)
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
                }, writeToProviders: true);
        }

        public static string DetectEnvironment()
        {
            var executionPath = AppDomain.CurrentDomain.BaseDirectory;

            if (executionPath.Contains("bin", StringComparison.OrdinalIgnoreCase) ||
                executionPath.Contains("Debug", StringComparison.OrdinalIgnoreCase))
            {
                return Environments.Development;
            }

            return Environments.Production;
        }

        private static void ConfigureJsonFiles(HostBuilderContext context, IConfigurationBuilder config)
        {
            var currentEnv = context.HostingEnvironment.EnvironmentName;

            config.AddJsonFile("logging.json", optional: false, reloadOnChange: true);
            config.AddJsonFile($"logging.{currentEnv}.json", optional: true, reloadOnChange: true);

            if (context.HostingEnvironment.IsDevelopment())
            {
                var rawUserName = Environment.UserName;
                var cleanChars = rawUserName.Where(char.IsLetterOrDigit).ToArray();
                var cleanUserName = new string(cleanChars);

                if (string.IsNullOrWhiteSpace(cleanUserName))
                {
                    cleanUserName = "Developer";
                }

                config.AddJsonFile($"logging.Development.{cleanUserName}.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsettings.Development.{cleanUserName}.json", optional: true, reloadOnChange: true);
            }
        }
    }

    internal class LifetimeLoggingService : IHostedService
    {
        public LifetimeLoggingService(IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
