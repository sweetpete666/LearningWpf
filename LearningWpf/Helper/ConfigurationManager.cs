using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Linq;

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

            var builder = Host.CreateDefaultBuilder();
            var environment = DetectEnvironment();

            return builder
                .UseEnvironment(environment)
                .ConfigureAppConfiguration(ConfigureJsonFiles)
                .ConfigureLogging(ConfigureLogging);
        }

        // Hilfsmethode, damit die App.xaml.cs die Konsole beim Beenden sauber schließen kann
        public static void ShutDownConsole()
        {
            consoleManager.TerminateConsole();
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

        private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging)
        {
            logging.ClearProviders();

            Log.Logger = new LoggerConfiguration()
             .ReadFrom.Configuration(context.Configuration)
             .CreateLogger();
            logging.AddSerilog();
        }
    }
}
