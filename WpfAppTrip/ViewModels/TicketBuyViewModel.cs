using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Models;
using WpfAppTrip.Services;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace WpfAppTrip.ViewModels
{
    public class TicketBuyViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
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

        public ICommand BackCommand { get; }
        public ICommand ContinueCommand { get; }

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
        #endregion

        public TicketBuyViewModel(INavigationService navigationService, IDialogService dialogService, FlightOptionViewModel selectedFlight)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _selectedFlight = selectedFlight;
            
            // Инициализация команд
            BackCommand = new RelayCommand(param => GoBack());
            ContinueCommand = new RelayCommand(param => ContinuePurchase(), param => CanContinuePurchase());
            
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
            // Здесь будет логика оформления покупки
            _dialogService.ShowInfo($"Билет успешно оформлен!\n\nПассажир: {PassengerLastName} {PassengerFirstName}\nМаршрут: {RouteInfo}\nСтоимость: {TotalPriceFormatted}");
            
            // Возвращаемся на главную страницу
            _navigationService.NavigateToWelcome();
        }
    }
} 