using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LearningWpf.Models;

namespace LearningWpf.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // ── Alle verfügbaren Daten ────────────────────────────────────────────
        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<JobGroup> AvailableJobGroups { get; } = new();
        public ObservableCollection<Job> AvailableJobs { get; } = new();

        // ── Ausgewählte Werte ─────────────────────────────────────────────────
        private User? _selectedUser;
        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                UpdateJobGroups();
            }
        }

        private JobGroup? _selectedJobGroup;
        public JobGroup? SelectedJobGroup
        {
            get => _selectedJobGroup;
            set
            {
                _selectedJobGroup = value;
                OnPropertyChanged();
                UpdateJobs();
            }
        }

        private Job? _selectedJob;
        public Job? SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged();
            }
        }

        // ── Konstruktor mit Beispieldaten ─────────────────────────────────────
        public MainWindowViewModel()
        {
            LoadSampleData();
        }

        private void LoadSampleData()
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

            var groupDev     = new JobGroup { Id = 1, Name = "Entwicklung",  Description = "Software entwickeln", Jobs = jobDevelopment };
            var groupDesign  = new JobGroup { Id = 2, Name = "Design",       Description = "UX & UI gestalten",   Jobs = jobDesign };
            var groupMgmt    = new JobGroup { Id = 3, Name = "Management",   Description = "Projekte steuern",    Jobs = jobManagement };

            Users.Add(new User
            {
                Id = 1, Name = "Anna Müller", Email = "anna@example.com",
                JobGroups = new List<JobGroup> { groupDev, groupDesign }
            });
            Users.Add(new User
            {
                Id = 2, Name = "Ben Schmidt", Email = "ben@example.com",
                JobGroups = new List<JobGroup> { groupMgmt }
            });
            Users.Add(new User
            {
                Id = 3, Name = "Clara Weber", Email = "clara@example.com",
                JobGroups = new List<JobGroup> { groupDev, groupDesign, groupMgmt }
            });
        }

        // ── Aktualisierungslogik ──────────────────────────────────────────────
        private void UpdateJobGroups()
        {
            AvailableJobGroups.Clear();
            SelectedJobGroup = null;

            if (_selectedUser is null) return;

            foreach (var group in _selectedUser.JobGroups)
                AvailableJobGroups.Add(group);
        }

        private void UpdateJobs()
        {
            AvailableJobs.Clear();
            SelectedJob = null;

            if (_selectedJobGroup is null) return;

            foreach (var job in _selectedJobGroup.Jobs)
                AvailableJobs.Add(job);
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
