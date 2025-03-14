using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Models;
using WpfAppTrip.Services;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Threading.Tasks;

namespace WpfAppTrip.ViewModels
{
    public class TicketsHistoryViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly Db.Dbhelper _db;
        
        private ObservableCollection<TicketData> _tickets;
        private TicketData _selectedTicket;
        private bool _isLoading;
        
        public ObservableCollection<TicketData> Tickets
        {
            get => _tickets;
            set => SetProperty(ref _tickets, value);
        }
        
        public TicketData SelectedTicket
        {
            get => _selectedTicket;
            set => SetProperty(ref _selectedTicket, value);
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        public ICommand BackCommand { get; }
        public ICommand OpenTicketCommand { get; }
        public ICommand RefreshCommand { get; }
        
        public TicketsHistoryViewModel(INavigationService navigationService, IDialogService dialogService, Db.Dbhelper db)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _db = db;
            
            Tickets = new ObservableCollection<TicketData>();
            
            // Инициализация команд
            BackCommand = new RelayCommand(param => GoBack());
            OpenTicketCommand = new RelayCommand(param => OpenTicket(), param => CanOpenTicket());
            RefreshCommand = new RelayCommand(async param => await LoadTicketsAsync());
            
            // Загрузка билетов
            Task.Run(async () => await LoadTicketsAsync());
        }
        
        private async Task LoadTicketsAsync()
        {
            try
            {
                IsLoading = true;
                
                await Task.Delay(500); // Имитация загрузки
                
                // Получаем билеты из базы данных
                var tickets = _db.CurrentUser != null 
                    ? _db.GetUserTickets(_db.CurrentUser.UserID) 
                    : new List<TicketData>();
                
                // Обновляем коллекцию в UI-потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Tickets.Clear();
                    foreach (var ticket in tickets)
                    {
                        Tickets.Add(ticket);
                    }
                    
                    Debug.WriteLine($"Загружено {tickets.Count} билетов");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке билетов: {ex.Message}");
                _dialogService.ShowError($"Не удалось загрузить билеты: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void GoBack()
        {
            _navigationService.GoBack();
        }
        
        private bool CanOpenTicket()
        {
            return SelectedTicket != null && 
                   !string.IsNullOrEmpty(SelectedTicket.TicketFilePath) && 
                   File.Exists(SelectedTicket.TicketFilePath);
        }
        
        private void OpenTicket()
        {
            try
            {
                if (CanOpenTicket())
                {
                    // Открываем файл с помощью ассоциированной программы
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = SelectedTicket.TicketFilePath,
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