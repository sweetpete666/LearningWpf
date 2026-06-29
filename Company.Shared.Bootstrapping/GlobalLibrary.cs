using System.Text.Json;

namespace Company.Shared.Bootstrapping
{
    public class GlobalLibrary
    {
        private static GlobalLibrary? _instance;
        public static GlobalLibrary Instance => _instance ??= new GlobalLibrary();

        private readonly GlobalLibrarySettings _settings = new();
        private const string ConfigFileName = "globallibrary.json";

        private GlobalLibrary()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, ConfigFileName);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Settings file for GlobalLibrary not found: {fullPath}");

            try
            {
                string jsonString = File.ReadAllText(fullPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var deserialized = JsonSerializer.Deserialize<GlobalLibrarySettings>(jsonString, options);

                if (deserialized?.Paths != null)
                    _settings.Paths = deserialized.Paths;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error reading {ConfigFileName}: {ex.Message}", ex);
            }
        }

        public string GetPath(string nameOfPath)
        {
            if (_settings.Paths.TryGetValue(nameOfPath, out var path)) return path;
            throw new KeyNotFoundException($"No path defined for name '{nameOfPath}' in {ConfigFileName}.");
        }

        public IEnumerable<string> GetPaths(IEnumerable<string> namesOfPaths)
        {
            foreach (var name in namesOfPaths)
            {
                yield return GetPath(name);
            }
        }

        public IEnumerable<string> GetPaths(params string[] namesOfPaths)
        {
            foreach (var name in namesOfPaths)
            {
                yield return GetPath(name);
            }
        }
    }

    public class GlobalLibrarySettings
    {
        public Dictionary<string, string> Paths { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
