using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace LearningWpf.Helper
{
    public class ConsoleManager
    {
        public class ConsoleSettings
        {
            public int X { get; set; } = 1920; // Standardwert (Startet auf dem 2. Monitor rechts)
            public int Y { get; set; } = 100;
            public int Width { get; set; } = 900;
            public int Height { get; set; } = 500;
        }

        // ── Win32-API Strukturen und Befehle ─────────────────────────────────
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        // ──────────────────────────────────────────────────────────────────────

        private static readonly string SettingsPath = "consolestate.json";
        private IntPtr consoleHandle = IntPtr.Zero;

        public void InitializeConsole()
        {
            // 1. Konsole erstellen
            AllocConsole();
            this.consoleHandle = GetConsoleWindow();

            if (this.consoleHandle != IntPtr.Zero)
            {
                // 2. Letzte Position laden
                ConsoleSettings settings = LoadSettings();

                // 3. Konsole positionieren
                MoveWindow(this.consoleHandle, settings.X, settings.Y, settings.Width, settings.Height, true);
            }
        }

        public void TerminateConsole()
        {
            // 4. Position vor dem Schließen speichern
            if (this.consoleHandle != IntPtr.Zero)
            {
                RECT rect;
                if (GetWindowRect(this.consoleHandle, out rect))
                {
                    ConsoleSettings currentSettings = new()
                    {
                        X = rect.Left,
                        Y = rect.Top,
                        Width = rect.Right - rect.Left,
                        Height = rect.Bottom - rect.Top
                    };
                    SaveSettings(currentSettings);
                }
            }

            // 5. Konsole freigeben
            FreeConsole();
        }

        private ConsoleSettings LoadSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                return new ConsoleSettings();
            }

            try
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<ConsoleSettings>(json) ?? new ConsoleSettings();
            }
            catch
            {
                return new ConsoleSettings();
            }
        }

        private void SaveSettings(ConsoleSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Fehler beim Speichern der UI-Settings ignorieren
            }
        }
    }
}
