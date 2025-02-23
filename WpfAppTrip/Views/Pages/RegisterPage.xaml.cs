using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using WpfAppTrip.Services;
using System;
using WpfAppTrip.ViewModels;
using Unity;

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

        public RegisterPage()
        {
            InitializeComponent();
            _authService = new AuthService();
            DataContext = App.Container.Resolve<RegisterViewModel>();
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

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterButton.IsEnabled = false;
            try
            {
                string username = PhoneTextBox.Text; // Используем телефон как имя пользователя
                string email = EmailTextBox.Text;
                string password = PasswordBox.Password;

                if (password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Пароли не совпадают", 
                                  "Ошибка", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                var (success, error) = await _authService.RegisterAsync(username, email, password);

                if (success)
                {
                    MessageBox.Show("Регистрация успешна!", 
                                  "Успех", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    
                    if (NavigationService?.CanGoBack == true)
                        NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show(error,
                                  "Ошибка регистрации",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при регистрации: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                RegisterButton.IsEnabled = true;
            }
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
