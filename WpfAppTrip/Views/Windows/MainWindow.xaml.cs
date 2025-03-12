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
            
            // Настройка навигации
            var navigationService = new NavigationService(MainFrame);
            
            // Проверка авторизации
            UpdateUserInfo();
            
            // Начальная страница
            MainFrame.Navigate(new SurveyPage());
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

        private void UpdateUserInfo()
        {
            if (_authService.CurrentUser != null)
            {
                UserNameTextBlock.Text = _authService.CurrentUser.Username;
                UserRoleTextBlock.Text = _authService.CurrentUser.Role;
                
                // Показать/скрыть элементы в зависимости от роли
                AdminPanelButton.Visibility = _authService.CurrentUser.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                UserNameTextBlock.Text = "Гость";
                UserRoleTextBlock.Text = "";
                AdminPanelButton.Visibility = Visibility.Collapsed;
            }
        }
        
        private void SurveyButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SurveyPage());
        }
        
        private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminPage());
        }
        
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _authService.Logout();
            UpdateUserInfo();
            MainFrame.Navigate(new SurveyPage());
        }
        
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Login());
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
