using LearningWpf.Models; // Wichtig für den AppSettings-Typ
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LearningWpf.Helper
{
    public static class ConfigurationManager
    {
        // 1. Die Instanz des ConsoleManager wird hier zentral als statisches Feld gehalten
        private static readonly ConsoleManager consoleManager = new();

        public static IHostBuilder CreateHostBuilder()
        {
            // 2. TIMING-FIX: Konsole SOFORT erstellen, noch VOR dem Builder!
            // Dadurch existiert das Windows-Fenster-Handle rechtzeitig für Serilog.
            consoleManager.InitializeConsole();

            var environment = DetectEnvironment();
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", environment);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);

            var builder = Host.CreateDefaultBuilder();
            

            return builder
                .UseEnvironment(environment)
                .ConfigureAppConfiguration(ConfigureJsonFiles)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices((context, services) =>
                {
                    // Das unschöne xxxxx(context) sauber durch den .NET-Standard ersetzt:
                    services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));

                    // Registrierung des automatischen Shutdown-Services
                    services.AddHostedService<HostShutdownCleanupService>();

                    if (context.Configuration is IConfigurationRoot configRoot)
                        LogConfiguration(context, configRoot);
                });
        }

        private static void LogConfiguration(HostBuilderContext context, IConfigurationRoot configRoot)
        {
            Console.WriteLine("Loaded Configuration sources:");
            foreach (var provider in configRoot.Providers)
            {
                var keys = provider.GetChildKeys(Enumerable.Empty<string>(), null);
                var keyCount = keys.Count();
                var providerName = provider.GetType().Name;
                var sourceInfo = provider.ToString() ?? providerName;
                if (keyCount > 0) 
                    Console.WriteLine($"{sourceInfo}: {keyCount} values");
            }
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

            // config.Sources.Clear();
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            config.SetBasePath(basePath);

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

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging)
        {
            logging.ClearProviders();

            Log.Logger = new LoggerConfiguration()
             .ReadFrom.Configuration(context.Configuration)
             .CreateLogger();
            logging.AddSerilog();
        }

        internal static void Shutdown()
        {
            Log.CloseAndFlush();
            consoleManager.TerminateConsole(); // Direkt auf das Feld zugreifen
        }

        /// <summary>
        /// Interner Service, der sich beim AppHost-Stop vollautomatisch um den Shutdown kümmert.
        /// </summary>
        private class HostShutdownCleanupService : IHostedService
        {
            public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

            public Task StopAsync(CancellationToken cancellationToken)
            {
                Shutdown();
                return Task.CompletedTask;
            }
        }
    }
}