using System;
using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Models;
using WpfAppTrip.Services;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WpfAppTrip.Views.Windows;

namespace WpfAppTrip.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly Db.Dbhelper _db;
        
        private string _username;
        private string _email;
        private string _phone;
        private ObservableCollection<TicketData> _userTickets;

        public ICommand SaveChangesCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand NavigateToTicketsHistoryCommand { get; }
        public ICommand OpenTicketCommand { get; }
        public ICommand RefreshTicketsCommand { get; }

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
        
        public ObservableCollection<TicketData> UserTickets
        {
            get => _userTickets;
            set => SetProperty(ref _userTickets, value);
        }

        public ProfileViewModel(AuthService authService, IDialogService dialogService, INavigationService navigationService, Db.Dbhelper db)
        {
            _authService = authService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _db = db;

            UserTickets = new ObservableCollection<TicketData>();

            SaveChangesCommand = new RelayCommand(async param => await SaveChangesAsync());
            LogoutCommand = new RelayCommand(param => Logout());
            NavigateToTicketsHistoryCommand = new RelayCommand(param => _navigationService.NavigateToTicketsHistory());
            OpenTicketCommand = new RelayCommand(param => OpenTicket(param as TicketData), param => CanOpenTicket(param as TicketData));
            RefreshTicketsCommand = new RelayCommand(async param => await LoadUserTicketsAsync());

            LoadUserData();
            Task.Run(async () => await LoadUserTicketsAsync());
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

        private async Task LoadUserTicketsAsync()
        {
            try
            {
                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    Debug.WriteLine("LoadUserTicketsAsync: Текущий пользователь не найден");
                    return;
                }

                Debug.WriteLine($"LoadUserTicketsAsync: Загрузка билетов для пользователя {user.Username} (ID: {user.UserID})");

                // Добавляем небольшую задержку для имитации асинхронной операции
                await Task.Delay(100);

                // Получаем последние 3 билета пользователя
                var tickets = _db.GetUserTickets(user.UserID);
                
                Debug.WriteLine($"LoadUserTicketsAsync: Получено {tickets.Count} билетов из базы данных");
                
                // Ограничиваем количество билетов до 3 для отображения в профиле
                var recentTickets = tickets.Count > 3 ? tickets.GetRange(0, 3) : tickets;
                
                Debug.WriteLine($"LoadUserTicketsAsync: Отображаем {recentTickets.Count} последних билетов");

                // Обновляем коллекцию в UI-потоке
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UserTickets.Clear();
                    foreach (var ticket in recentTickets)
                    {
                        UserTickets.Add(ticket);
                        Debug.WriteLine($"LoadUserTicketsAsync: Добавлен билет {ticket.TicketNumber}, маршрут: {ticket.RouteInfo}");
                    }
                    
                    Debug.WriteLine($"LoadUserTicketsAsync: Всего билетов в коллекции: {UserTickets.Count}");
                    
                    // Вызываем обновление свойства, чтобы UI обновился
                    OnPropertyChanged(nameof(UserTickets));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке билетов: {ex.Message}");
            }
        }

        private async Task SaveChangesAsync()
        {
            var user = _authService.GetCurrentUser();
            if (user == null) return;

            // Обновляем основную информацию
            var infoResult = await _authService.UpdateUserInfoAsync(
                user.UserID, 
                Email, 
                user.LastName ?? "", 
                user.FirstName ?? "", 
                user.MiddleName);
                
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
            // Выполняем выход из системы
            _authService.Logout();
            
            try
            {
                // Создаем новое окно входа
                var loginWindow = new LoginWindow();
                
                // Устанавливаем его как главное окно приложения
                Application.Current.MainWindow = loginWindow;
                
                // Показываем окно входа
                loginWindow.Show();
                
                // Находим текущее окно, из которого выполняется выход
                Window currentWindow = null;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.IsActive && window.GetType() != typeof(LoginWindow))
                    {
                        currentWindow = window;
                        break;
                    }
                }
                
                // Закрываем текущее окно, если оно найдено и не является окном входа
                if (currentWindow != null && currentWindow.GetType() != typeof(LoginWindow))
                {
                    currentWindow.Close();
                }
                
                // Сообщаем пользователю об успешном выходе
                _dialogService.ShowInfo("Вы успешно вышли из системы");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при выходе из системы: {ex.Message}");
                _dialogService.ShowError($"Ошибка при выходе из системы: {ex.Message}");
            }
        }
        
        private bool CanOpenTicket(TicketData ticket)
        {
            return ticket != null && 
                   !string.IsNullOrEmpty(ticket.TicketFilePath) && 
                   File.Exists(ticket.TicketFilePath);
        }
        
        private void OpenTicket(TicketData ticket)
        {
            try
            {
                if (CanOpenTicket(ticket))
                {
                    // Открываем файл с помощью ассоциированной программы
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = ticket.TicketFilePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    _dialogService.ShowError("Файл билета не найден.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при открытии билета: {ex.Message}");
                _dialogService.ShowError($"Не удалось открыть билет: {ex.Message}");
            }
        }
    }
} 