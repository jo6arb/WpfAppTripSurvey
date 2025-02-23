using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.RegularExpressions;
using WpfAppTrip.Commands;
using WpfAppTrip.Services;
using System.Windows.Controls;
using System.Windows;
using WpfAppTrip.Views.Pages;
using WpfAppTrip.Views.Controls;

namespace WpfAppTrip.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private string _phone = "+7";
        private string _email;
        private string _password;
        private string _confirmPassword;
        private bool _isRegisterEnabled;

        public RegisterViewModel(
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

            RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                    ValidateFields();
            }
        }

        public bool IsRegisterEnabled
        {
            get => _isRegisterEnabled;
            private set => SetProperty(ref _isRegisterEnabled, value);
        }

        public ICommand BackCommand { get; }
        public ICommand RegisterCommand { get; }

        private void ValidateFields()
        {
            var phoneRegex = new Regex(@"^\+7\d{10}$");
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            bool isPhoneValid = !string.IsNullOrEmpty(Phone) && phoneRegex.IsMatch(Phone);
            bool isEmailValid = !string.IsNullOrEmpty(Email) && emailRegex.IsMatch(Email);
            bool isPasswordValid = !string.IsNullOrEmpty(Password) && Password.Length >= 6;
            bool isConfirmPasswordValid = Password == ConfirmPassword;

            IsRegisterEnabled = isPhoneValid && isEmailValid && isPasswordValid && isConfirmPasswordValid;
        }

        private async Task RegisterAsync()
        {
            IsRegisterEnabled = false;
            try
            {
                var (success, error) = await _authService.RegisterAsync(Phone, Email, Password);

                if (success)
                {
                    _dialogService.ShowInfo("Регистрация успешна!");
                    _navigationService.GoBack();
                }
                else
                {
                    _dialogService.ShowError(error);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Ошибка при регистрации: {ex.Message}");
            }
            finally
            {
                IsRegisterEnabled = true;
            }
        }
    }
} 