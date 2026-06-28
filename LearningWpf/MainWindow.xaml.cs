using LearningWpf.ViewModels;
using System.Windows;

namespace LearningWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            this.DataContext = viewModel;
            InitializeComponent();
        }
    }
}