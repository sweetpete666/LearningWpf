using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Company.Shared.Bootstrapping
{
    public class AppBootstrapper
    {
        private static AppBootstrapper? _instance;
        public static AppBootstrapper Instance => _instance ??= new AppBootstrapper();

        private List<string> jsonFiles = [];

        // 1. Die Instanz des ConsoleManager wird hier zentral als statisches Feld gehalten
        private static readonly ConsoleManager consoleManager = new();

        private AppBootstrapper() { }

        public IHostBuilder CreateHostBuilder()
        {
            // 2. TIMING-FIX: Konsole SOFORT erstellen, noch VOR dem Builder!
            // Dadurch existiert das Windows-Fenster-Handle rechtzeitig für Serilog.
            consoleManager.InitializeConsole();

            return Host.CreateDefaultBuilder()
                .UseEnvironment(DetectEnvironment())
                .ConfigureAppConfiguration(ConfigureJsonFiles)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices((context, services) =>
                {
                    // Registrierung des automatischen Shutdown-Services
                    services.AddHostedService<HostShutdownCleanupService>();

                    if (context.Configuration is IConfigurationRoot configRoot)
                    {
                        LogConfiguration(context, configRoot);
                    }
                });
        }

        public AppBootstrapper AddJsonFile(string filePath)
        {
            if (!jsonFiles.Contains(filePath))
            {
                jsonFiles.Add(filePath);
            }
            return this;
        }
        public AppBootstrapper AddJsonFiles(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                AddJsonFile(filePath);
            }
            return this;
        }

        private static void LogConfiguration(HostBuilderContext context, IConfigurationRoot configRoot)
        {
            Console.WriteLine("Loaded Configuration sources:");
            List<string> notLoaded = [];
            foreach (var provider in configRoot.Providers)
            {
                var keys = provider.GetChildKeys(Enumerable.Empty<string>(), null);
                var keyCount = keys.Count();
                var providerName = provider.GetType().Name;
                var sourceInfo = provider.ToString() ?? providerName;
                if (keyCount > 0)
                    Console.WriteLine($"{sourceInfo}: {keyCount} values");
                else
                    notLoaded.Add(sourceInfo);
            }
            if (notLoaded.Count > 0) Console.WriteLine($"Not loaded: {string.Join(",", notLoaded)}");
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
            // var basePath = AppDomain.CurrentDomain.BaseDirectory;
            // config.SetBasePath(basePath);

            var currentEnv = context.HostingEnvironment.EnvironmentName;

            config.AddJsonFile("logging.json", optional: true, reloadOnChange: true);
            config.AddJsonFile($"logging.{currentEnv}.json", optional: true, reloadOnChange: true);

            foreach (var jsonFile in Instance.jsonFiles)
            {
                config.AddJsonFile(jsonFile, optional: true, reloadOnChange: true);
                config.AddJsonFile(ExtendBy(jsonFile, currentEnv), optional: true, reloadOnChange: true);
            }

            if (context.HostingEnvironment.IsDevelopment())
            {
                var rawUserName = Environment.UserName;
                var cleanChars = rawUserName.Where(char.IsLetterOrDigit).ToArray();
                var cleanUserName = new string(cleanChars);

                if (string.IsNullOrWhiteSpace(cleanUserName))
                {
                    cleanUserName = "Developer";
                }

                config.AddJsonFile($"logging.{currentEnv}.{cleanUserName}.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{currentEnv}.{cleanUserName}.json", optional: true, reloadOnChange: true);
                foreach (var jsonFile in Instance.jsonFiles)
                {
                    config.AddJsonFile(ExtendBy(jsonFile, $"{currentEnv}.{cleanUserName}"), optional: true, reloadOnChange: true);
                }
            }

            config.AddSubstitution();
        }

        private static string ExtendBy(string filePath, string subExtension)
        {
            var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var envFileName = $"{fileNameWithoutExt}.{subExtension}{extension}";
            var envFilePath = Path.Combine(directory, envFileName);
            return envFilePath;
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