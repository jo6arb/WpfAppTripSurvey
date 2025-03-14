using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.Services;
using WpfAppTrip.ViewModels;
using Unity;
using System.Diagnostics;
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
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly LoginViewModel _viewModel;

        public Login()
        {
            InitializeComponent();
            
            // Получаем сервисы через внедрение зависимостей
            _authService = App.Container.Resolve<AuthService>();
            _navigationService = App.Container.Resolve<INavigationService>();
            _dialogService = App.Container.Resolve<IDialogService>();
            _viewModel = App.Container.Resolve<LoginViewModel>();
            
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
            if (PhoneTextBox == null || EmailTextBox == null || PasswordBox == null || LoginButton == null)
                return;

            bool isPhoneValid = !string.IsNullOrEmpty(PhoneTextBox.Text) && _phoneRegex.IsMatch(PhoneTextBox.Text);
            bool isEmailValid = !string.IsNullOrEmpty(EmailTextBox.Text) && _emailRegex.IsMatch(EmailTextBox.Text);
            bool isPasswordValid = !string.IsNullOrEmpty(PasswordBox.Password) && PasswordBox.Password.Length >= 6;

            LoginButton.IsEnabled = isPhoneValid && isEmailValid && isPasswordValid;
            
            // Обновляем ViewModel
            if (_viewModel != null)
            {
                _viewModel.Phone = PhoneTextBox.Text;
                _viewModel.Email = EmailTextBox.Text;
                _viewModel.Password = PasswordBox.Password;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginButton.IsEnabled = false;
            
            try
            {
                var (success, error) = await _authService.LoginAsync(PhoneTextBox.Text, PasswordBox.Password);
                
                if (success)
                {
                    // Обновляем главное окно, чтобы отразить изменения в авторизации
                    var mainViewModel = App.Container.Resolve<MainViewModel>();
                    mainViewModel.UpdateAllProperties();
                    
                    // Показываем главное окно
                    _navigationService.ShowMainWindow();
                    
                    // Явно переходим на страницу приветствия
                    _navigationService.NavigateToWelcome();
                    
                    // Закрываем окно логина
                    var loginWindow = Window.GetWindow(this) as LoginWindow;
                    loginWindow?.Close();
                }
                else
                {
                    _dialogService.ShowError(error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при входе: {ex.Message}");
                _dialogService.ShowError($"Ошибка при входе: {ex.Message}");
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToPage("Register");
        }
    }
}
