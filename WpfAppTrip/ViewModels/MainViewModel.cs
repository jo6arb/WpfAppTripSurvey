using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Services;

namespace WpfAppTrip.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly AuthService _authService;
        private readonly IDialogService _dialogService;
        private bool _isWelcomePageVisible = true;

        public MainViewModel(
            INavigationService navigationService,
            AuthService authService,
            IDialogService dialogService)
        {
            _navigationService = navigationService;
            _authService = authService;
            _dialogService = dialogService;

            NavigateToSurveyCommand = new RelayCommand(_ => NavigateToSurvey());
            NavigateToAdminCommand = new RelayCommand(_ => NavigateToAdmin(), _ => IsAdmin);
            LogoutCommand = new RelayCommand(_ => Logout());

            // Обновляем состояние при создании
            UpdateAllProperties();
        }

        public bool IsUserLoggedIn => AuthService.CurrentUser != null;
        public bool IsAdmin => AuthService.CurrentUser?.IsAdmin ?? false;
        public string CurrentUserName => AuthService.CurrentUser?.Username ?? string.Empty;

        public bool IsWelcomePageVisible
        {
            get => _isWelcomePageVisible;
            set => SetProperty(ref _isWelcomePageVisible, value);
        }

        public ICommand NavigateToSurveyCommand { get; }
        public ICommand NavigateToAdminCommand { get; }
        public ICommand LogoutCommand { get; }

        private void NavigateToSurvey()
        {
            IsWelcomePageVisible = false;
            _navigationService.NavigateToPage("Survey");
            OnPropertyChanged(nameof(IsWelcomePageVisible));
        }

        private void NavigateToAdmin()
        {
            if (IsAdmin)
            {
                IsWelcomePageVisible = false;
                _navigationService.NavigateToPage("Admin");
            }
            else
            {
                _dialogService.ShowWarning("У вас нет прав для доступа к этой странице");
            }
        }

        private void Logout()
        {
            if (_dialogService.ShowConfirm("Вы действительно хотите выйти?"))
            {
                _authService.Logout();
                _navigationService.ShowLoginWindow();
            }
        }

        public void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(IsUserLoggedIn));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(IsWelcomePageVisible));
        }
    }
} 