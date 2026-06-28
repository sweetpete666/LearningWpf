using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace LearningWpf.Helper
{
    public static class ConfigurationManager
    {
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

        public static void ConfigureJsonFiles(HostBuilderContext context, IConfigurationBuilder config)
        {
            // ── 1. LOGGING-PIPELINE ──────────────────────────────────────────
            // Basis-Logging (Pflicht)
            config.AddJsonFile("logging.json", optional: false, reloadOnChange: true);

            // Umgebungs-Logging (z.B. logging.Development.json)
            var currentEnv = context.HostingEnvironment.EnvironmentName;
            var envLoggingJson = $"logging.{currentEnv}.json";
            config.AddJsonFile(envLoggingJson, optional: true, reloadOnChange: true);


            // ── 2. ENTWICKLER-SPECIFIC OVERRIDES (Nur im Development) ─────────
            if (context.HostingEnvironment.IsDevelopment())
            {
                // Sauberen Benutzernamen ohne Sonderzeichen generieren
                var rawUserName = Environment.UserName;
                var cleanChars = rawUserName.Where(char.IsLetterOrDigit).ToArray();
                var cleanUserName = new string(cleanChars);

                if (string.IsNullOrWhiteSpace(cleanUserName))
                {
                    cleanUserName = "Developer";
                }

                // A) Entwicklerspezifisches LOGGING laden (z.B. logging.Development.SmithHarry.json)
                var developerLoggingJson = $"logging.Development.{cleanUserName}.json";
                config.AddJsonFile(developerLoggingJson, optional: true, reloadOnChange: true);

                // B) Entwicklerspezifische APP-SETTINGS laden (z.B. appsettings.Development.SmithHarry.json)
                var developerAppJson = $"appsettings.Development.{cleanUserName}.json";
                config.AddJsonFile(developerAppJson, optional: true, reloadOnChange: true);
            }
        }
    }
}
