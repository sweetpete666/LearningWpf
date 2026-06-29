using Microsoft.Win32; // Unbedingt für den Registry-Zugriff hinzufügen!
using System.Runtime.InteropServices;

namespace Company.Shared.Bootstrapping
{
    public class ConsoleManager
    {
        public class ConsoleSettings
        {
            public int X { get; set; } = 1920;
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

        // Der Pfad in der Windows-Registry für Ihre App
        private static readonly string RegistrySubKey = @"Software\LearningWpf\ConsoleSettings";
        private IntPtr consoleHandle = IntPtr.Zero;

        public void InitializeConsole()
        {
            AllocConsole();
            consoleHandle = GetConsoleWindow();

            if (consoleHandle != IntPtr.Zero)
            {
                ConsoleSettings settings = LoadSettings();
                MoveWindow(consoleHandle, settings.X, settings.Y, settings.Width, settings.Height, true);
            }
        }

        public void TerminateConsole()
        {
            if (consoleHandle != IntPtr.Zero)
            {
                RECT rect;
                if (GetWindowRect(consoleHandle, out rect))
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
            FreeConsole();
        }

        private ConsoleSettings LoadSettings()
        {
            ConsoleSettings settings = new();

            try
            {
                // Öffnet den Registry-Schlüssel zum Lesen
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistrySubKey);

                if (key != null)
                {
                    // Holt die Werte und nutzt die Standardwerte, falls ein Eintrag fehlt
                    settings.X = (int)(key.GetValue("X", settings.X) ?? settings.X);
                    settings.Y = (int)(key.GetValue("Y", settings.Y) ?? settings.Y);
                    settings.Width = (int)(key.GetValue("Width", settings.Width) ?? settings.Width);
                    settings.Height = (int)(key.GetValue("Height", settings.Height) ?? settings.Height);
                }
            }
            catch
            {
                // Bei Fehlern (z.B. Leserechten) greifen die Standardwerte der Klasse
            }

            return settings;
        }

        private void SaveSettings(ConsoleSettings settings)
        {
            try
            {
                // Erstellt oder öffnet den Pfad mit Schreibrechten
                using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistrySubKey);

                if (key != null)
                {
                    key.SetValue("X", settings.X);
                    key.SetValue("Y", settings.Y);
                    key.SetValue("Width", settings.Width);
                    key.SetValue("Height", settings.Height);
                }
            }
            catch
            {
                // Fehler beim Schreiben in die Registry sicher ignorieren
            }
        }
    }
}
