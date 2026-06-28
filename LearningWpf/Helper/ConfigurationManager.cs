using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace LearningWpf.Helper
{
    public static class ConfigurationManager
    {
        /// <summary>
        /// Ermittelt die Umgebung (Development/Production) automatisch anhand des Ausführungspfads.
        /// </summary>
        public static string DetectEnvironment()
        {
            var executionPath = AppDomain.CurrentDomain.BaseDirectory;

            // Wenn der Pfad "bin" oder "Debug" enthält, sind wir am Entwickler-PC
            if (executionPath.Contains("bin", StringComparison.OrdinalIgnoreCase) ||
                executionPath.Contains("Debug", StringComparison.OrdinalIgnoreCase))
            {
                return Environments.Development;
            }

            return Environments.Production;
        }

        /// <summary>
        /// Konfiguriert die JSON-Dateien-Pipeline inklusive dem Entwickler-spezifischen Override.
        /// </summary>
        public static void ConfigureJsonFiles(HostBuilderContext context, IConfigurationBuilder config)
        {
            // Nur im Development-Modus macht das Entwickler-JSON Sinn
            if (context.HostingEnvironment.IsDevelopment())
            {
                var rawUserName = Environment.UserName;
                var cleanChars = rawUserName.Where(char.IsLetterOrDigit).ToArray();
                var cleanUserName = new string(cleanChars);
                if (string.IsNullOrWhiteSpace(cleanUserName))
                {
                    cleanUserName = "Developer";
                }
                var developerJson = $"appsettings.Development.{cleanUserName}.json";
                config.AddJsonFile(developerJson, optional: true, reloadOnChange: true);
            }
        }
    }
}
