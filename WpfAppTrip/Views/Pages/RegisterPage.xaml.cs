using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using WpfAppTrip.Services;
using System;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private readonly Regex _phoneRegex = new Regex(@"^\+7\d{10}$");
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;

        public RegisterPage()
        {
            InitializeComponent();
            _authService = new AuthService();
            _navigationService = new NavigationService(null); // Здесь нужно передать Frame
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

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Отключаем кнопку на время проверки и регистрации
            RegisterButton.IsEnabled = false;

            // Получаем данные из полей
            string phone = PhoneTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Проверяем валидность данных
            if (!ValidateRegistrationData(phone, email, password, confirmPassword))
            {
                RegisterButton.IsEnabled = true;
                return;
            }

            try
            {
                // Регистрируем пользователя
                bool success = await _authService.RegisterAsync(phone, password, email);
                
                if (success)
                {
                    MessageBox.Show("Регистрация успешна! Теперь вы можете войти в систему.",
                                  "Успех",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    // Переходим на страницу входа
                    _navigationService.Navigate(new Login());
                }
                else
                {
                    MessageBox.Show("Не удалось зарегистрироваться. Возможно, пользователь с таким номером телефона уже существует.",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    RegisterButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                RegisterButton.IsEnabled = true;
            }
        }

        private bool ValidateRegistrationData(string phone, string email, string password, string confirmPassword)
        {
            // Очищаем поля от ошибок
            PhoneTextBox.ClearValue(Border.BorderBrushProperty);
            EmailTextBox.ClearValue(Border.BorderBrushProperty);
            PasswordBox.ClearValue(Border.BorderBrushProperty);
            ConfirmPasswordBox.ClearValue(Border.BorderBrushProperty);
            RegisterButton.IsEnabled = true;

            bool isValid = true;

            // Проверка телефона (должен быть в формате +7XXXXXXXXXX)
            if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^\+7\d{10}$"))
            {
                PhoneTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                isValid = false;
            }

            // Проверка email
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                EmailTextBox.BorderBrush = System.Windows.Media.Brushes.Red;
                isValid = false;
            }

            // Проверка пароля (минимум 6 символов)
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                PasswordBox.BorderBrush = System.Windows.Media.Brushes.Red;
                isValid = false;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                ConfirmPasswordBox.BorderBrush = System.Windows.Media.Brushes.Red;
                isValid = false;
            }

            return isValid;
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.Navigate(new Login());
        }

        private void ValidateFields(object sender, RoutedEventArgs e)
        {
            if (PhoneTextBox == null || EmailTextBox == null || PasswordBox == null ||
                ConfirmPasswordBox == null || RegisterButton == null)
                return;

            bool isPhoneValid = !string.IsNullOrEmpty(PhoneTextBox.Text) && _phoneRegex.IsMatch(PhoneTextBox.Text);
            bool isEmailValid = !string.IsNullOrEmpty(EmailTextBox.Text) && _emailRegex.IsMatch(EmailTextBox.Text);
            bool isPasswordValid = !string.IsNullOrEmpty(PasswordBox.Password) && PasswordBox.Password.Length >= 6;
            bool isConfirmPasswordValid = !string.IsNullOrEmpty(ConfirmPasswordBox.Password) &&
                                        PasswordBox.Password == ConfirmPasswordBox.Password;

            RegisterButton.IsEnabled = isPhoneValid && isEmailValid && isPasswordValid && isConfirmPasswordValid;
        }

        private async Task YourMethodNameAsync()
        {
            await Task.Delay(1); // Или другая асинхронная операция
            // ... ваш код ...
        }

        private void YourMethodName()
        {
            // ... ваш код ...
        }
    }
}
