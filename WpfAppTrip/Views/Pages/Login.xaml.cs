using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.Views.Windows;

namespace WpfAppTrip.Views
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        private readonly Regex _phoneRegex = new Regex(@"^\+7\d{10}$");
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        public Login()
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

        private void ValidateFields(object sender, RoutedEventArgs e)
        {
            if (PhoneTextBox == null || EmailTextBox == null || PasswordBox == null || LoginButton == null)
                return;

            bool isPhoneValid = !string.IsNullOrEmpty(PhoneTextBox.Text) && _phoneRegex.IsMatch(PhoneTextBox.Text);
            bool isEmailValid = !string.IsNullOrEmpty(EmailTextBox.Text) && _emailRegex.IsMatch(EmailTextBox.Text);
            bool isPasswordValid = !string.IsNullOrEmpty(PasswordBox.Password) && PasswordBox.Password.Length >= 6;

            LoginButton.IsEnabled = isPhoneValid && isEmailValid && isPasswordValid;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            try
            {
                // Используем телефон или email для входа
                string loginValue = _phoneRegex.IsMatch(PhoneTextBox.Text) ? PhoneTextBox.Text : EmailTextBox.Text;
                bool success = true;

                if (success)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.",
                                  "Ошибка входа",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }
            }
            catch
            {
                MessageBox.Show("Произошла ошибка при входе. Пожалуйста, попробуйте позже.",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                ValidateFields(sender, new RoutedEventArgs());
            }
        }
    }
}
