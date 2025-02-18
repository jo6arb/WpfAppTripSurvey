using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfAppTrip.Services;
using WpfAppTrip.Views.Pages;

namespace WpfAppTrip.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private bool _isWelcomePageVisible = true;

        public MainWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
            DataContext = this;

            // Инициализация команд
            NavigateToSurveyCommand = new RelayCommand(param => StartSurveyButton_Click(null, null));
            NavigateToAdminCommand = new RelayCommand(param => NavigateToAdmin());
            LogoutCommand = new RelayCommand(param => Logout());
        }

        public bool IsUserLoggedIn => AuthService.CurrentUser != null;
        public bool IsAdmin => AuthService.CurrentUser?.IsAdmin ?? false;
        public string CurrentUserName => AuthService.CurrentUser?.Username ?? string.Empty;
        
        public ICommand NavigateToSurveyCommand { get; }
        public ICommand NavigateToAdminCommand { get; }
        public ICommand LogoutCommand { get; }
        
        public bool IsWelcomePageVisible
        {
            get => _isWelcomePageVisible;
            set
            {
                _isWelcomePageVisible = value;
                OnPropertyChanged();
            }
        }

        private void StartSurveyButton_Click(object sender, RoutedEventArgs e)
        {
            IsWelcomePageVisible = false;
            MainFrame.Navigate(new SurveyPage());
        }

        private void NavigateToAdmin()
        {
            IsWelcomePageVisible = false;
            MainFrame.Navigate(new AdminPage());
        }

        private void Logout()
        {
            _authService.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Простая реализация ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
