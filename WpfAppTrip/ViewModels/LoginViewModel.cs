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

        public LoginViewModel(
            AuthService authService, 
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            
            BackCommand = new RelayCommand(_ => 
            {
                var frame = Application.Current.MainWindow?.FindName("MainFrame") as Frame;
                if (frame != null)
                {
                    frame.Navigate(new WelcomeControl());
                }
            });

            LoginCommand = new RelayCommand(async _ => await LoginAsync());
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

        public ICommand BackCommand { get; }
        public ICommand LoginCommand { get; }

        private void ValidateFields()
        {
            var phoneRegex = new Regex(@"^\+7\d{10}$");
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            bool isPhoneValid = !string.IsNullOrEmpty(Phone) && phoneRegex.IsMatch(Phone);
            bool isEmailValid = !string.IsNullOrEmpty(Email) && emailRegex.IsMatch(Email);
            bool isPasswordValid = !string.IsNullOrEmpty(Password) && Password.Length >= 6;

            IsLoginEnabled = isPhoneValid && isEmailValid && isPasswordValid;
        }

        private bool CanLogin(object parameter)
        {
            return IsLoginEnabled;
        }

        private async Task LoginAsync()
        {
            IsLoginEnabled = false;
            try
            {
                var (success, error) = await _authService.LoginAsync(Email, Password);
                if (success)
                {
                    _navigationService.ShowMainWindow();
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
                IsLoginEnabled = true;
            }
        }
    }
} 