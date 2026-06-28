using LearningWpf.Models;

namespace LearningWpf.Repositories
{
    public class MockUserRepository : IUserRepository
    {
        private readonly List<User> mockUsers = new();

        public MockUserRepository()
        {
            InitializeMockData();
        }

        public List<User> GetAllUsers()
        {
            return this.mockUsers;
        }

        private void InitializeMockData()
        {
            var jobDevelopment = new List<Job>
            {
                new() { Id = 1, Name = "C# entwickeln",      Description = "Backend-Logik in C# schreiben" },
                new() { Id = 2, Name = "XAML gestalten",     Description = "WPF-Oberflächen erstellen" },
                new() { Id = 3, Name = "Unit Tests",         Description = "Tests schreiben und ausführen" },
            };

            var jobDesign = new List<Job>
            {
                new() { Id = 4, Name = "Mockups erstellen",  Description = "UI-Entwürfe in Figma" },
                new() { Id = 5, Name = "Design-System",      Description = "Farben, Schriften, Komponenten" },
            };

            var jobManagement = new List<Job>
            {
                new() { Id = 6, Name = "Sprint planen",      Description = "Aufgaben für den Sprint einteilen" },
                new() { Id = 7, Name = "Backlog pflegen",    Description = "Anforderungen priorisieren" },
                new() { Id = 8, Name = "Standup moderieren", Description = "Tägliches Meeting leiten" },
            };

            var groupDev = new JobGroup { Id = 1, Name = "Entwicklung", Description = "Software entwickeln", Jobs = jobDevelopment };
            var groupDesign = new JobGroup { Id = 2, Name = "Design", Description = "UX & UI gestalten", Jobs = jobDesign };
            var groupMgmt = new JobGroup { Id = 3, Name = "Management", Description = "Projekte steuern", Jobs = jobManagement };

            this.mockUsers.Add(new User
            {
                Id = 1,
                Name = "Anna Müller",
                Email = "anna@example.com",
                JobGroups = new List<JobGroup> { groupDev, groupDesign }
            });

            this.mockUsers.Add(new User
            {
                Id = 2,
                Name = "Ben Schmidt",
                Email = "ben@example.com",
                JobGroups = new List<JobGroup> { groupMgmt }
            });

            this.mockUsers.Add(new User
            {
                Id = 3,
                Name = "Clara Weber",
                Email = "clara@example.com",
                JobGroups = new List<JobGroup> { groupDev, groupDesign, groupMgmt }
            });
        }
    }
}
