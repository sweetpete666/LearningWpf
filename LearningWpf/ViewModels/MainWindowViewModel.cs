using LearningWpf.Models;
using LearningWpf.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LearningWpf.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // ── Alle verfügbaren Daten ────────────────────────────────────────────
        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<JobGroup> AvailableJobGroups { get; } = new();
        public ObservableCollection<Job> AvailableJobs { get; } = new();

        public string XXX { get; set; }


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
        private readonly ILogger<MainWindowViewModel> logger;
        private readonly IUserRepository userRepository;
        private readonly AppSettings settings;
        public MainWindowViewModel(IOptions<AppSettings> options, ILogger<MainWindowViewModel> logger, IUserRepository userRepository)
        {
            this.logger = logger;
            this.userRepository = userRepository;
            settings = options.Value;
            XXX = settings.XXX ?? "nicht definiert";

            if (logger.IsEnabled(LogLevel.Debug))
                logger.LogDebug("MainWindowViewModel created with AppSettings: {@AppSettings}", settings);
            LoadData();
        }

        private void LoadData()
        {
            logger.LogInformation("Fetching data from UserRepository.");

            // Daten direkt aus dem Repository laden statt lokal zu generieren
            var loadedUsers = userRepository.GetAllUsers();

            foreach (var user in loadedUsers)
            {
                Users.Add(user);
            }
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
