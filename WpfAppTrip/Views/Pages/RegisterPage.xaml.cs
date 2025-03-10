using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using WpfAppTrip.Services;
using System;
using WpfAppTrip.ViewModels;
using Unity;
using System.Diagnostics;

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
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly RegisterViewModel _viewModel;

        public RegisterPage()
        {
            InitializeComponent();
            
            // Получаем сервисы через внедрение зависимостей
            _authService = App.Container.Resolve<AuthService>();
            _navigationService = App.Container.Resolve<INavigationService>();
            _dialogService = App.Container.Resolve<IDialogService>();
            _viewModel = App.Container.Resolve<RegisterViewModel>();
            
            DataContext = _viewModel;
            
            // Инициализация поля телефона с +7
            if (PhoneTextBox != null && string.IsNullOrEmpty(PhoneTextBox.Text))
            {
                PhoneTextBox.Text = "+7";
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ValidateFields(sender, e);
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
            if (PhoneTextBox == null || EmailTextBox == null || PasswordBox == null ||
                ConfirmPasswordBox == null || RegisterButton == null)
                return;

            bool isPhoneValid = !string.IsNullOrEmpty(PhoneTextBox.Text) && _phoneRegex.IsMatch(PhoneTextBox.Text);
            bool isEmailValid = !string.IsNullOrEmpty(EmailTextBox.Text) && _emailRegex.IsMatch(EmailTextBox.Text);
            bool isPasswordValid = !string.IsNullOrEmpty(PasswordBox.Password) && PasswordBox.Password.Length >= 6;
            bool isConfirmPasswordValid = !string.IsNullOrEmpty(ConfirmPasswordBox.Password) &&
                                        PasswordBox.Password == ConfirmPasswordBox.Password;

            RegisterButton.IsEnabled = isPhoneValid && isEmailValid && isPasswordValid && isConfirmPasswordValid;
            
            // Обновляем ViewModel
            if (_viewModel != null)
            {
                _viewModel.Phone = PhoneTextBox.Text;
                _viewModel.Email = EmailTextBox.Text;
                _viewModel.Password = PasswordBox.Password;
                _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
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
                    _dialogService.ShowWarning("Пароли не совпадают");
                    return;
                }

                var (success, error) = await _authService.RegisterAsync(username, email, password);

                if (success)
                {
                    _dialogService.ShowInfo("Регистрация успешна!");
                    
                    // Автоматически логинимся
                    var (loginSuccess, loginError) = await _authService.LoginAsync(email, password);
                    if (loginSuccess)
                    {
                        // Обновляем главное окно
                        var mainViewModel = App.Container.Resolve<MainViewModel>();
                        mainViewModel?.UpdateAllProperties();
                        
                        // Переходим к главной странице
                        _navigationService.NavigateToPage("Survey");
                    }
                    else
                    {
                        _dialogService.ShowInfo("Пожалуйста, войдите с вашими новыми данными");
                        _navigationService.NavigateToPage("Login");
                    }
                }
                else
                {
                    _dialogService.ShowError(error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при регистрации: {ex.Message}");
                _dialogService.ShowError($"Произошла ошибка при регистрации: {ex.Message}");
            }
            finally
            {
                RegisterButton.IsEnabled = true;
            }
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
