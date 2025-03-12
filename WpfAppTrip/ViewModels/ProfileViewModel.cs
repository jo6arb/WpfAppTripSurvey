using System;
using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Models;
using WpfAppTrip.Services;
using System.Threading.Tasks;

namespace WpfAppTrip.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly IDialogService _dialogService;
        private string _username;
        private string _email;
        private string _phone;

        public ICommand SaveChangesCommand { get; }
        public ICommand LogoutCommand { get; }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public ProfileViewModel(AuthService authService, IDialogService dialogService)
        {
            _authService = authService;
            _dialogService = dialogService;

            SaveChangesCommand = new RelayCommand(async param => await SaveChangesAsync());
            LogoutCommand = new RelayCommand(param => Logout());

            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = _authService.GetCurrentUser();
            if (user != null)
            {
                Username = user.Username;
                Email = user.Email;
                Phone = user.Phone;
            }
        }

        private async Task SaveChangesAsync()
        {
            var user = _authService.GetCurrentUser();
            if (user == null) return;

            // Обновляем основную информацию
            var infoResult = await _authService.UpdateUserInfoAsync(user.UserID, Username, Email);
            if (!infoResult.Success)
            {
                _dialogService.ShowError(infoResult.Error);
                return;
            }

            // Обновляем телефон
            var phoneResult = await _authService.UpdateUserPhoneAsync(user.UserID, Phone);
            if (!phoneResult.Success)
            {
                _dialogService.ShowError(phoneResult.Error);
                return;
            }

            _dialogService.ShowInfo("Данные профиля успешно обновлены");
        }

        private void Logout()
        {
            _authService.Logout();
        }
    }
} 