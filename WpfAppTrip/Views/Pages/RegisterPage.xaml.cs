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
            // Очистка полей при загрузке страницы
            LastNameTextBox.Clear();
            FirstNameTextBox.Clear();
            MiddleNameTextBox.Clear();
            EmailTextBox.Clear();
            PhoneTextBox.Text = "+7";
            PasswordBox.Clear();
            ConfirmPasswordBox.Clear();
            
            ValidateFields(null, null);
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

        private void ValidateFields(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.LastName = LastNameTextBox.Text;
                _viewModel.FirstName = FirstNameTextBox.Text;
                _viewModel.MiddleName = MiddleNameTextBox.Text;
                _viewModel.Email = EmailTextBox.Text;
                _viewModel.Phone = PhoneTextBox.Text;
                _viewModel.Password = PasswordBox.Password;
                _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
                
                RegisterButton.IsEnabled = _viewModel.IsRegisterEnabled;
            }
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateFields(sender, e);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = PasswordBox.Password;
                _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
                
                RegisterButton.IsEnabled = _viewModel.IsRegisterEnabled;
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterButton.IsEnabled = false;
            try
            {
                string phone = PhoneTextBox.Text;
                string email = EmailTextBox.Text;
                string lastName = LastNameTextBox.Text;
                string firstName = FirstNameTextBox.Text;
                string middleName = MiddleNameTextBox.Text;
                string password = PasswordBox.Password;

                if (password != ConfirmPasswordBox.Password)
                {
                    _dialogService.ShowWarning("Пароли не совпадают");
                    return;
                }

                var (success, error) = await _authService.RegisterAsync(phone, email, password, lastName, firstName, middleName);

                if (success)
                {
                    _dialogService.ShowInfo("Регистрация успешна!");
                    
                    // Проверяем, откуда пришел пользователь
                    var frame = Window.GetWindow(this)?.FindName("MainFrame") as Frame;
                    var previousPage = frame?.NavigationService?.CanGoBack == true ? 
                        frame.NavigationService.Content : null;
                    
                    // Если пришли со страницы администратора, возвращаемся на нее
                    if (previousPage is AdminPage)
                    {
                        _navigationService.NavigateToPage("Admin");
                        return;
                    }
                    
                    // Иначе стандартное поведение - автоматический логин
                    var (loginSuccess, loginError) = await _authService.LoginAsync(phone, password);
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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Переходим на страницу входа
            _navigationService.NavigateToPage("Login");
        }
    }
}
