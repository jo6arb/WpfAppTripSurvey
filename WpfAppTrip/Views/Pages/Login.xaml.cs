using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.Services;
using WpfAppTrip.ViewModels;
using Unity;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private readonly Regex _phoneRegex = new Regex(@"^\+7\d{10}$");
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private readonly AuthService _authService;
        private System.Windows.Controls.TextBox _phoneTextBox;
        private System.Windows.Controls.TextBox _emailTextBox;
        private System.Windows.Controls.PasswordBox _passwordBox;
        private System.Windows.Controls.Button _loginButton;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly LoginViewModel _viewModel;

        public Login()
        {
            InitializeComponent();
            _authService = new AuthService();
            _navigationService = App.Container.Resolve<INavigationService>();
            _dialogService = App.Container.Resolve<IDialogService>();
            _viewModel = App.Container.Resolve<LoginViewModel>();
            DataContext = _viewModel;
            
            // Инициализация элементов управления
            _phoneTextBox = (System.Windows.Controls.TextBox)FindName("PhoneTextBox");
            _emailTextBox = (System.Windows.Controls.TextBox)FindName("EmailTextBox");
            _passwordBox = (System.Windows.Controls.PasswordBox)FindName("PasswordBox");
            _loginButton = (System.Windows.Controls.Button)FindName("LoginButton");

            // Привязка пароля через событие, так как PasswordBox не поддерживает привязку
            if (PasswordBox != null)
            {
                PasswordBox.PasswordChanged += (s, e) =>
                {
                    if (_viewModel != null)
                        _viewModel.Password = PasswordBox.Password;
                };
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ValidateFields(sender, new RoutedEventArgs());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем Frame из окна
            var frame = Window.GetWindow(this)?.FindName("MainFrame") as Frame;
            if (frame != null)
            {
                // Очищаем историю навигации
                while (frame.CanGoBack)
                {
                    frame.RemoveBackEntry();
                }
                // Очищаем текущую страницу, чтобы показать стартовый контент
                frame.Content = null;
            }
        }

        private void ValidateFields(object sender, RoutedEventArgs e)
        {
            if (_phoneTextBox == null || _emailTextBox == null || _passwordBox == null || _loginButton == null)
                return;

            bool isPhoneValid = !string.IsNullOrEmpty(_phoneTextBox.Text) && _phoneRegex.IsMatch(_phoneTextBox.Text);
            bool isEmailValid = !string.IsNullOrEmpty(_emailTextBox.Text) && _emailRegex.IsMatch(_emailTextBox.Text);
            bool isPasswordValid = !string.IsNullOrEmpty(_passwordBox.Password) && _passwordBox.Password.Length >= 6;

            _loginButton.IsEnabled = isPhoneValid && isEmailValid && isPasswordValid;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.LoginCommand.Execute(null);
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToPage("Register");
        }
    }
}
