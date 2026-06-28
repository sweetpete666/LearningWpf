using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Linq;

namespace LearningWpf.Helper
{
    public static class ConfigurationManager
    {
        // ── NEU: DER GESAMTE BUILDER WIRD HIER ERZEUGT UND KONFIGURIERT ──────
        public static IHostBuilder CreateHostBuilder()
        {
            // 1. Den rohen Standard-Builder instanziieren
            var builder = Host.CreateDefaultBuilder();

            // 2. Umgebung ermitteln
            var environment = DetectEnvironment();

            // 3. Alle Teilschritte anhängen und den konfigurierten Builder zurückgeben
            return builder
                .UseEnvironment(environment)
                .ConfigureAppConfiguration(ConfigureJsonFiles)
                .ConfigureLogging(ConfigureLogging);
        }

        private static string DetectEnvironment()
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
            config.AddJsonFile("logging.json", optional: false, reloadOnChange: true);

            var currentEnv = context.HostingEnvironment.EnvironmentName;
            var envLoggingJson = $"logging.{currentEnv}.json";
            config.AddJsonFile(envLoggingJson, optional: true, reloadOnChange: true);

            if (context.HostingEnvironment.IsDevelopment())
            {
                var rawUserName = Environment.UserName;
                var cleanChars = rawUserName.Where(char.IsLetterOrDigit).ToArray();
                var cleanUserName = new string(cleanChars);

                if (string.IsNullOrWhiteSpace(cleanUserName))
                {
                    cleanUserName = "Developer";
                }

                var developerLoggingJson = $"logging.Development.{cleanUserName}.json";
                config.AddJsonFile(developerLoggingJson, optional: true, reloadOnChange: true);

                var developerAppJson = $"appsettings.Development.{cleanUserName}.json";
                config.AddJsonFile(developerAppJson, optional: true, reloadOnChange: true);
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
