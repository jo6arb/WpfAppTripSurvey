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

namespace WpfAppTrip.ViewModels
{
    public class TicketBuyViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IPdfTicketService _pdfService;
        private readonly Db.Dbhelper _db;
        private readonly FlightOptionViewModel _selectedFlight;
        
        // Данные пассажира
        private string _passengerLastName;
        private string _passengerFirstName;
        private string _passengerGender;
        private DateTime? _passengerBirthDate;
        private string _documentType;
        private string _documentNumber;
        private string _documentExpiryDate;
        
        // Данные покупателя
        private string _buyerLastName;
        private string _buyerFirstName;
        private string _buyerEmail;
        private string _buyerPhone;
        
        // Согласие на обработку данных
        private bool _isConsentGiven;
        
        // Информация о маршруте
        private string _routeInfo;
        private string _classInfo;
        private decimal _totalPrice;
        
        // Информация о билете
        private string _ticketNumber;
        private string _ticketFilePath;

        public ICommand BackCommand { get; }
        public ICommand ContinueCommand { get; }
        public ICommand OpenTicketCommand { get; }

        #region Свойства пассажира
        public string PassengerLastName
        {
            get => _passengerLastName;
            set => SetProperty(ref _passengerLastName, value);
        }

        public string PassengerFirstName
        {
            get => _passengerFirstName;
            set => SetProperty(ref _passengerFirstName, value);
        }

        public string PassengerGender
        {
            get => _passengerGender;
            set => SetProperty(ref _passengerGender, value);
        }

        public DateTime? PassengerBirthDate
        {
            get => _passengerBirthDate;
            set => SetProperty(ref _passengerBirthDate, value);
        }

        public string DocumentType
        {
            get => _documentType;
            set => SetProperty(ref _documentType, value);
        }

        public string DocumentNumber
        {
            get => _documentNumber;
            set => SetProperty(ref _documentNumber, value);
        }

        public string DocumentExpiryDate
        {
            get => _documentExpiryDate;
            set => SetProperty(ref _documentExpiryDate, value);
        }
        #endregion

        #region Свойства покупателя
        public string BuyerLastName
        {
            get => _buyerLastName;
            set => SetProperty(ref _buyerLastName, value);
        }

        public string BuyerFirstName
        {
            get => _buyerFirstName;
            set => SetProperty(ref _buyerFirstName, value);
        }

        public string BuyerEmail
        {
            get => _buyerEmail;
            set => SetProperty(ref _buyerEmail, value);
        }

        public string BuyerPhone
        {
            get => _buyerPhone;
            set => SetProperty(ref _buyerPhone, value);
        }
        #endregion

        #region Прочие свойства
        public bool IsConsentGiven
        {
            get => _isConsentGiven;
            set => SetProperty(ref _isConsentGiven, value);
        }

        public string RouteInfo
        {
            get => _routeInfo;
            set => SetProperty(ref _routeInfo, value);
        }

        public string ClassInfo
        {
            get => _classInfo;
            set => SetProperty(ref _classInfo, value);
        }

        public decimal TotalPrice
        {
            get => _totalPrice;
            set => SetProperty(ref _totalPrice, value);
        }

        public string TotalPriceFormatted => $"{TotalPrice:N0}₽";
        
        public string TicketNumber
        {
            get => _ticketNumber;
            set => SetProperty(ref _ticketNumber, value);
        }
        
        public string TicketFilePath
        {
            get => _ticketFilePath;
            set => SetProperty(ref _ticketFilePath, value);
        }
        #endregion

        public TicketBuyViewModel(
            INavigationService navigationService, 
            IDialogService dialogService, 
            IPdfTicketService pdfService,
            Db.Dbhelper db,
            FlightOptionViewModel selectedFlight)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _pdfService = pdfService;
            _db = db;
            _selectedFlight = selectedFlight;
            
            // Инициализация команд
            BackCommand = new RelayCommand(param => GoBack());
            ContinueCommand = new RelayCommand(param => ContinuePurchase(), param => CanContinuePurchase());
            OpenTicketCommand = new RelayCommand(param => OpenTicket(), param => !string.IsNullOrEmpty(TicketFilePath));
            
            // Инициализация данных
            InitializeData();
        }

        private void InitializeData()
        {
            // Получаем страну назначения и код аэропорта
            string destination = _selectedFlight?.Destination ?? "Пхукет";
            string destinationCode = GetDestinationCode(destination);
            
            // Устанавливаем информацию о маршруте
            RouteInfo = $"Москва - {destination}";
            ClassInfo = "8 июль, 1 класс Эконом";
            TotalPrice = _selectedFlight?.Price ?? 185282; // Цена из выбранного рейса или по умолчанию
            
            Debug.WriteLine($"Инициализация данных для маршрута: {RouteInfo}, код аэропорта: {destinationCode}");
            
            // Генерируем номер билета
            TicketNumber = $"TKT{new Random().Next(100000, 999999)}";
            
            // Если пользователь авторизован, заполняем данные покупателя
            if (_db.CurrentUser != null)
            {
                BuyerLastName = _db.CurrentUser.GetLastName();
                BuyerFirstName = _db.CurrentUser.GetFirstName();
                BuyerEmail = _db.CurrentUser.Email;
                BuyerPhone = _db.CurrentUser.Phone;
                
                // Можно также заполнить данные пассажира, если это тот же человек
                PassengerLastName = _db.CurrentUser.GetLastName();
                PassengerFirstName = _db.CurrentUser.GetFirstName();
            }
        }

        // Метод для получения кода аэропорта по названию страны/города
        private string GetDestinationCode(string destination)
        {
            // Словарь соответствия стран/городов и кодов аэропортов
            var airportCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Пхукет", "HKT" },
                { "Бали", "DPS" },
                { "Турция", "IST" },
                { "Стамбул", "IST" },
                { "Анталия", "AYT" },
                { "Италия", "FCO" },
                { "Рим", "FCO" },
                { "Милан", "MXP" },
                { "Франция", "CDG" },
                { "Париж", "CDG" },
                { "Испания", "MAD" },
                { "Мадрид", "MAD" },
                { "Барселона", "BCN" },
                { "Греция", "ATH" },
                { "Афины", "ATH" },
                { "Таиланд", "BKK" },
                { "Бангкок", "BKK" }
            };
            
            // Возвращаем код аэропорта или первые 3 буквы названия страны/города
            if (airportCodes.TryGetValue(destination, out string code))
            {
                return code;
            }
            
            // Если код не найден, используем первые 3 буквы названия
            return destination.Length >= 3 ? destination.Substring(0, 3).ToUpper() : "HKT";
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        private bool CanContinuePurchase()
        {
            // Проверяем, что все обязательные поля заполнены и согласие дано
            return !string.IsNullOrEmpty(PassengerLastName) &&
                   !string.IsNullOrEmpty(PassengerFirstName) &&
                   PassengerBirthDate.HasValue &&
                   !string.IsNullOrEmpty(DocumentNumber) &&
                   !string.IsNullOrEmpty(BuyerLastName) &&
                   !string.IsNullOrEmpty(BuyerFirstName) &&
                   !string.IsNullOrEmpty(BuyerEmail) &&
                   !string.IsNullOrEmpty(BuyerPhone) &&
                   IsConsentGiven;
        }

        private void ContinuePurchase()
        {
            try
            {
                // Создаем объект с данными билета
                var ticketData = new TicketData
                {
                    // Информация о пассажире
                    PassengerLastName = PassengerLastName,
                    PassengerFirstName = PassengerFirstName,
                    PassengerGender = PassengerGender,
                    PassengerBirthDate = PassengerBirthDate,
                    DocumentType = DocumentType,
                    DocumentNumber = DocumentNumber,
                    DocumentExpiryDate = DocumentExpiryDate,
                    
                    // Информация о покупателе
                    BuyerLastName = BuyerLastName,
                    BuyerFirstName = BuyerFirstName,
                    BuyerEmail = BuyerEmail,
                    BuyerPhone = BuyerPhone,
                    
                    // Информация о маршруте
                    RouteInfo = RouteInfo,
                    ClassInfo = ClassInfo,
                    TotalPrice = TotalPrice,
                    
                    // Информация о билете
                    TicketNumber = TicketNumber,
                    PurchaseDate = DateTime.Now
                };
                
                // Генерируем PDF-билет
                Debug.WriteLine("Генерация PDF-билета...");
                string pdfPath = _pdfService.GenerateTicket(ticketData);
                
                if (!string.IsNullOrEmpty(pdfPath))
                {
                    // Сохраняем путь к файлу
                    TicketFilePath = pdfPath;
                    ticketData.TicketFilePath = pdfPath;
                    
                    // Сохраняем информацию о билете в базу данных
                    Debug.WriteLine("Сохранение информации о билете в базу данных...");
                    int ticketId = _db.SaveTicket(ticketData);
                    
                    if (ticketId > 0)
                    {
                        Debug.WriteLine($"Билет успешно сохранен в базе данных с ID: {ticketId}");
                        
                        // Показываем сообщение об успешной покупке
                        var result = _dialogService.ShowQuestion(
                            $"Билет успешно оформлен!\n\nПассажир: {PassengerLastName} {PassengerFirstName}\nМаршрут: {RouteInfo}\nСтоимость: {TotalPriceFormatted}\n\nБилет сохранен по пути:\n{pdfPath}\n\nОткрыть билет сейчас?",
                            "Билет оформлен");
                        
                        if (result)
                        {
                            // Открываем билет
                            OpenTicket();
                        }
                        
                        // Возвращаемся на главную страницу
                        _navigationService.NavigateToWelcome();
                    }
                    else
                    {
                        _dialogService.ShowError("Не удалось сохранить информацию о билете в базе данных. Пожалуйста, попробуйте еще раз.");
                    }
                }
                else
                {
                    _dialogService.ShowError("Не удалось создать PDF-билет. Пожалуйста, попробуйте еще раз.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при оформлении билета: {ex.Message}");
                _dialogService.ShowError($"Произошла ошибка при оформлении билета: {ex.Message}");
            }
        }
        
        private void OpenTicket()
        {
            try
            {
                if (!string.IsNullOrEmpty(TicketFilePath) && File.Exists(TicketFilePath))
                {
                    // Открываем файл с помощью ассоциированной программы
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = TicketFilePath,
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