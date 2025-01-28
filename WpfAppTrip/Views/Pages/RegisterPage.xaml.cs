using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private readonly Regex _phoneRegex = new Regex(@"^\+7\d{10}$");
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        public RegisterPage()
        {
            InitializeComponent();
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
            RegisterButton.IsEnabled = false;
            try
            {
                bool success = true;

                if (success)
                {
                    MessageBox.Show("Регистрация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (NavigationService?.CanGoBack == true)
                        NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("Пользователь с таким телефоном или email уже существует.",
                                  "Ошибка регистрации",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
            catch
            {
                MessageBox.Show("Произошла ошибка при регистрации. Пожалуйста, попробуйте позже.",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                ValidateFields(sender, new RoutedEventArgs());
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
