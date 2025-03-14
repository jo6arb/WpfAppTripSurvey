using System.Windows.Input;
using WpfAppTrip.Services;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using WpfAppTrip.Commands;
using System;
using System.Windows.Controls;
using System.Windows;
using WpfAppTrip.Views.Pages;
using WpfAppTrip.Views.Controls;

namespace WpfAppTrip.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private string _phone = "+7";
        private string _email;
        private string _password;
        private bool _isLoginEnabled;
        private bool _isLoading;

        public LoginViewModel(
            AuthService authService,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
            RegisterCommand = new RelayCommand(_ => _navigationService.NavigateToPage("RegisterPage"), _ => !IsLoading);
            BackCommand = new RelayCommand(_ => GoBack(), _ => !IsLoading);
        }

        public string Phone
        {
            get => _phone;
            set
            {
                if (SetProperty(ref _phone, value))
                    ValidateFields();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                    ValidateFields();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                    ValidateFields();
            }
        }

        public bool IsLoginEnabled
        {
            get => _isLoginEnabled;
            private set => SetProperty(ref _isLoginEnabled, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        private void ValidateFields()
        {
            IsLoginEnabled = !string.IsNullOrEmpty(Phone) && !string.IsNullOrEmpty(Password);
        }

        private void GoBack()
        {
            // Получаем Frame из окна
            var frame = Application.Current.MainWindow?.FindName("MainFrame") as Frame;
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

        private async Task LoginAsync()
        {
            IsLoading = true;
            IsLoginEnabled = false;
            try
            {
                var (success, error) = await _authService.LoginAsync(Phone, Password);

                if (success)
                {
                    _navigationService.NavigateToPage("MainPage");
                }
                else
                {
                    _dialogService.ShowError(error);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при входе: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                IsLoginEnabled = true;
            }
        }
    }
} 