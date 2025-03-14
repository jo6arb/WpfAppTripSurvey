using System;
using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Services;
using System.Diagnostics;
using WpfAppTrip.Models;

namespace WpfAppTrip.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly AuthService _authService;
        private readonly IDialogService _dialogService;
        private bool _isWelcomePageVisible = true;
        private bool _isProfilePanelVisible = false;

        public event Action NavigateToWelcomePage;
        public event Action NavigateToProfilePage;

        public MainViewModel(
            INavigationService navigationService,
            AuthService authService,
            IDialogService dialogService)
        {
            _navigationService = navigationService;
            _authService = authService;
            _dialogService = dialogService;

            // Инициализация команд
            NavigateToSurveyCommand = new RelayCommand(param => NavigateToSurvey());
            NavigateToAdminCommand = new RelayCommand(param => NavigateToAdmin(), param => CanNavigateToAdmin());
            LogoutCommand = new RelayCommand(param => Logout());
            ToggleProfilePanelCommand = new RelayCommand(param => ToggleProfilePanel());

            // Подписываемся на событие навигации
            _navigationService.Navigated += (sender, e) =>
            {
                _isWelcomePageVisible = false;
                OnPropertyChanged(nameof(IsWelcomePageVisible));
            };
        }

        public bool IsUserLoggedIn => _authService.GetCurrentUser() != null;
        public bool IsAdmin => _authService.IsCurrentUserAdmin();
        public string CurrentUserName => _authService.GetCurrentUser()?.Username ?? string.Empty;
        public User CurrentUser => _authService.GetCurrentUser();

        public bool IsWelcomePageVisible
        {
            get => _isWelcomePageVisible;
            set
            {
                _isWelcomePageVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsProfilePanelVisible
        {
            get => _isProfilePanelVisible;
            set
            {
                _isProfilePanelVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand NavigateToSurveyCommand { get; }
        public ICommand NavigateToAdminCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ToggleProfilePanelCommand { get; }

        private void ToggleProfilePanel()
        {
            IsProfilePanelVisible = !IsProfilePanelVisible;
            
            if (IsProfilePanelVisible)
            {
                NavigateToProfilePage?.Invoke();
            }
        }

        private void NavigateToSurvey()
        {
            Debug.WriteLine("Переход к опросу...");
            
            if (!IsUserLoggedIn)
            {
                _dialogService.ShowError("Для прохождения опроса необходимо войти в систему");
                _navigationService.ShowLoginWindow();
                return;
            }
            
            IsWelcomePageVisible = false;
            _navigationService.NavigateToPage("Survey");
            OnPropertyChanged(nameof(IsWelcomePageVisible));
            Debug.WriteLine("Навигация к опросу выполнена");
        }

        private void NavigateToAdmin()
        {
            Debug.WriteLine("Переход к админ-панели...");
            if (!_authService.IsCurrentUserAdmin())
            {
                _dialogService.ShowError("У вас нет прав администратора.");
                return;
            }

            IsWelcomePageVisible = false;
            _navigationService.NavigateToPage("Admin");
            OnPropertyChanged(nameof(IsWelcomePageVisible));
        }

        private bool CanNavigateToAdmin()
        {
            return _authService.IsCurrentUserAdmin();
        }

        private void Logout()
        {
            _authService.Logout();
            
            // Показываем окно входа через NavigationService
            _navigationService.ShowLoginWindow();
            
            // Обновляем свойства ViewModel
            IsWelcomePageVisible = true;
            IsProfilePanelVisible = false;
            OnPropertyChanged(nameof(IsUserLoggedIn));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUser));
        }

        public void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(IsUserLoggedIn));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUser));
        }

        public void ShowWelcomePage()
        {
            NavigateToWelcomePage?.Invoke();
        }
    }
} 