namespace LearningWpf.Models
{
    public class JobGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Job> Jobs { get; set; } = new();
    }
}