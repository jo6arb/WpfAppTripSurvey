using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.Services;
using WpfAppTrip.Views.Windows;

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

        public Login()
        {
            InitializeComponent();
            _authService = new AuthService();
            
            // Инициализация элементов управления
            _phoneTextBox = (System.Windows.Controls.TextBox)FindName("PhoneTextBox");
            _emailTextBox = (System.Windows.Controls.TextBox)FindName("EmailTextBox");
            _passwordBox = (System.Windows.Controls.PasswordBox)FindName("PasswordBox");
            _loginButton = (System.Windows.Controls.Button)FindName("LoginButton");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ValidateFields(sender, new RoutedEventArgs());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
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

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _loginButton.IsEnabled = false;
            try
            {
                string email = _emailTextBox.Text;
                string password = _passwordBox.Password;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                if (await _authService.LoginAsync(email, password))
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Window.GetWindow(this).Close();
                }
                else
                {
                    MessageBox.Show("Неверный email или пароль", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", 
                              "Ошибка", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
            finally
            {
                _loginButton.IsEnabled = true;
            }
        }
    }
}
