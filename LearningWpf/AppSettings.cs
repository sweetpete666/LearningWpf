using Company.Shared.Bootstrapping;

namespace LearningWpf
{
    public class AppSettings
    {
        public string ApplicationName { get; set; } = "XXX";
        public int MaxItemsPerPage { get; set; } = 20;
        public bool EnableFeatureX { get; set; }
        public EncryptedString? Encrypted { get; set; }
        public string? XXX { get; set; }
    }
}
