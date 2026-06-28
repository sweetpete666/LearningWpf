using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LearningWpf.Helper
{
    public class ConsoleStarter : IHostedService
    {
        private readonly ConsoleManager consoleManager;
        private readonly AppSettings settings;

        // Hier ist "this." weiterhin nötig, um den Parameter vom Feld zu unterscheiden
        public ConsoleStarter(ConsoleManager consoleManager, IOptions<AppSettings> options)
        {
            this.consoleManager = consoleManager;
            settings = options.Value; // Hier reicht bereits options.Value ohne this!
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Komplett ohne "this." – super sauber und direkt lesbar!
            if (settings.ShowConsole)
            {
                consoleManager.InitializeConsole();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (settings.ShowConsole)
            {
                consoleManager.TerminateConsole();
            }

            return Task.CompletedTask;
        }
    }
}
