namespace LearningWpf.Models
{
    internal class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public List<UserJobGroup> UserJobGroups { get; set; } = new();
    }
}
