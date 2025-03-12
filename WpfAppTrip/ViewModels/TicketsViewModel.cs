using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Models;
using WpfAppTrip.Services;
using System.Linq;
using System.Diagnostics;
using WpfAppTrip.Views.Pages;
using System.Collections.Generic;

namespace WpfAppTrip.ViewModels
{
    public class TicketsViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly Tour _selectedTour;
        private DateTime _selectedDate;
        private bool _isFlightSelected;
        private ObservableCollection<FlightViewModel> _availableFlights;
        private ObservableCollection<FlightOptionViewModel> _flightOptions;
        private FlightViewModel _selectedFlight;

        public ICommand BackCommand { get; }
        public ICommand SelectFlightCommand { get; }
        public ICommand BookFlightCommand { get; }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    LoadAvailableFlights();
                }
            }
        }

        public bool IsFlightSelected
        {
            get => _isFlightSelected;
            set => SetProperty(ref _isFlightSelected, value);
        }

        public ObservableCollection<FlightViewModel> AvailableFlights
        {
            get => _availableFlights;
            set => SetProperty(ref _availableFlights, value);
        }

        public ObservableCollection<FlightOptionViewModel> FlightOptions
        {
            get => _flightOptions;
            set => SetProperty(ref _flightOptions, value);
        }

        public FlightViewModel SelectedFlight
        {
            get => _selectedFlight;
            set => SetProperty(ref _selectedFlight, value);
        }

        public TicketsViewModel(INavigationService navigationService, IDialogService dialogService, Tour selectedTour)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _selectedTour = selectedTour;
            
            // Инициализация команд
            BackCommand = new RelayCommand(param => GoBack());
            SelectFlightCommand = new RelayCommand(param => SelectFlight(param as FlightViewModel));
            BookFlightCommand = new RelayCommand(param => BookFlight(param as FlightOptionViewModel));
            
            // Инициализация коллекций
            AvailableFlights = new ObservableCollection<FlightViewModel>();
            FlightOptions = new ObservableCollection<FlightOptionViewModel>();
            
            // Установка текущей даты
            SelectedDate = DateTime.Now.AddDays(7);
            
            // Создаем тестовый рейс для отображения
            CreateTestFlightOptions();
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }

        private void LoadAvailableFlights()
        {
            // Очищаем текущие рейсы
            AvailableFlights.Clear();
            IsFlightSelected = false;
            
            // Получаем страну назначения из тура или используем последнее слово из названия
            string destination = _selectedTour?.Destination;
            if (string.IsNullOrEmpty(destination))
            {
                string[] parts = _selectedTour.Name.Split(' ');
                destination = parts.Length > 0 ? parts[parts.Length - 1] : "Пхукет";
            }
            
            string destinationCode = GetDestinationCode(destination);
            Debug.WriteLine($"Загрузка рейсов для направления: {destination}, код: {destinationCode}");
            
            // Добавляем 5 тестовых рейсов
            AvailableFlights.Add(new FlightViewModel
            {
                Route = $"MOW - {destinationCode} - MOW",
                Price = Math.Round(_selectedTour.Price * 0.3m / 1000) * 1000,
                DateRange = $"ПТ {SelectedDate.Day} июль - ПТ {SelectedDate.AddDays(7).Day} июль",
                Passengers = "1 пассажир, эконом",
                TicketsFound = 397,
                Destination = destination
            });
            
            AvailableFlights.Add(new FlightViewModel
            {
                Route = $"MOW - {destinationCode} - MOW",
                Price = Math.Round(_selectedTour.Price * 0.25m / 1000) * 1000,
                DateRange = $"ПТ {SelectedDate.Day} июль - ПТ {SelectedDate.AddDays(10).Day} июль",
                Passengers = "1 пассажир, эконом",
                TicketsFound = 245,
                Destination = destination
            });
            
            AvailableFlights.Add(new FlightViewModel
            {
                Route = $"MOW - {destinationCode} - MOW",
                Price = Math.Round(_selectedTour.Price * 0.35m / 1000) * 1000,
                DateRange = $"ПТ {SelectedDate.Day} июль - ПТ {SelectedDate.AddDays(5).Day} июль",
                Passengers = "1 пассажир, эконом",
                TicketsFound = 156,
                Destination = destination
            });
            
            AvailableFlights.Add(new FlightViewModel
            {
                Route = $"MOW - {destinationCode} - MOW",
                Price = Math.Round(_selectedTour.Price * 0.4m / 1000) * 1000,
                DateRange = $"ПТ {SelectedDate.Day} июль - ПТ {SelectedDate.AddDays(14).Day} июль",
                Passengers = "1 пассажир, эконом",
                TicketsFound = 89,
                Destination = destination
            });
            
            AvailableFlights.Add(new FlightViewModel
            {
                Route = $"MOW - {destinationCode} - MOW",
                Price = Math.Round(_selectedTour.Price * 0.28m / 1000) * 1000,
                DateRange = $"ПТ {SelectedDate.Day} июль - ПТ {SelectedDate.AddDays(7).Day} июль",
                Passengers = "1 пассажир, эконом",
                TicketsFound = 312,
                Destination = destination
            });
            
            // Устанавливаем первый рейс как выбранный
            if (AvailableFlights.Count > 0)
            {
                SelectedFlight = AvailableFlights[0];
            }
        }

        private void CreateTestFlightOptions()
        {
            Debug.WriteLine("Создание тестовых вариантов рейсов");
            
            // Получаем страну назначения из тура или используем Пхукет по умолчанию
            string destination = _selectedTour?.Destination ?? "Пхукет";
            string destinationCode = GetDestinationCode(destination);
            
            Debug.WriteLine($"Страна назначения: {destination}, код: {destinationCode}");
            
            // Создаем тестовый рейс
            SelectedFlight = new FlightViewModel
            {
                Route = $"MOW - {destinationCode} - MOW",
                Price = 57657,
                DateRange = $"ПТ 8 июль - ПТ 19 июль",
                Passengers = "1 пассажир, эконом",
                TicketsFound = 397
            };
            
            // Очищаем текущие опции
            FlightOptions.Clear();
            
            // Генерируем тестовые опции рейсов
            FlightOptions.Add(new FlightOptionViewModel
            {
                Price = 57657,
                TravelTime = "58ч в пути",
                DepartureTime = "04:45",
                ArrivalTime = "15:20",
                DepartureAirport = "MOW",
                ArrivalAirport = destinationCode,
                Transfers = "1 пересадка",
                TransferAirports = "IST (Новый Аэропорт Стамбула)",
                BaggagePrice = 7625,
                BuyButtonText = "Купить",
                Destination = destination
            });
            
            FlightOptions.Add(new FlightOptionViewModel
            {
                Price = 51227,
                TravelTime = "51ч в пути",
                DepartureTime = "06:25",
                ArrivalTime = "01:40",
                DepartureAirport = destinationCode,
                ArrivalAirport = "MOW",
                Transfers = "1 пересадка",
                TransferAirports = "IST (Новый Аэропорт Стамбула)",
                BaggagePrice = 7625,
                BuyButtonText = "Купить за 65 282₽",
                Destination = destination
            });
            
            IsFlightSelected = true;
            Debug.WriteLine($"Создано {FlightOptions.Count} вариантов рейсов");
        }

        private void SelectFlight(FlightViewModel flight)
        {
            if (flight == null) return;
            
            SelectedFlight = flight;
            
            // Получаем страну назначения и код
            string destination = flight.Destination ?? "Пхукет";
            string destinationCode = GetDestinationCode(destination);
            
            // Очищаем текущие опции
            FlightOptions.Clear();
            
            // Генерируем тестовые опции рейсов
            FlightOptions.Add(new FlightOptionViewModel
            {
                Price = flight.Price,
                TravelTime = "58ч в пути",
                DepartureTime = "04:45",
                ArrivalTime = "15:20",
                DepartureAirport = "MOW",
                ArrivalAirport = destinationCode,
                Transfers = "1 пересадка",
                TransferAirports = "IST (Новый Аэропорт Стамбула)",
                BaggagePrice = 7625,
                BuyButtonText = "Купить",
                Destination = destination
            });
            
            FlightOptions.Add(new FlightOptionViewModel
            {
                Price = flight.Price + 2000,
                TravelTime = "51ч в пути",
                DepartureTime = "06:25",
                ArrivalTime = "01:40",
                DepartureAirport = destinationCode,
                ArrivalAirport = "MOW",
                Transfers = "1 пересадка",
                TransferAirports = "IST (Новый Аэропорт Стамбула)",
                BaggagePrice = 7625,
                BuyButtonText = "Купить",
                Destination = destination
            });
            
            FlightOptions.Add(new FlightOptionViewModel
            {
                Price = flight.Price - 1000,
                TravelTime = "56ч в пути",
                DepartureTime = "02:40",
                ArrivalTime = "15:20",
                DepartureAirport = "MOW",
                ArrivalAirport = destinationCode,
                Transfers = "2 пересадки",
                TransferAirports = "IST, AYT (Аэропорты Стамбула)",
                BaggagePrice = 8853,
                BuyButtonText = $"Купить за {flight.Price + 7625:N0}₽",
                Destination = destination
            });
            
            IsFlightSelected = true;
        }

        private void BookFlight(FlightOptionViewModel flightOption)
        {
            if (flightOption == null) return;
            
            try
            {
                // Переходим на страницу покупки билета
                var ticketBuyPage = new TicketBuy(flightOption);
                _navigationService.GetMainFrame()?.Navigate(ticketBuyPage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при переходе на страницу покупки билета: {ex.Message}");
                _dialogService.ShowError("Не удалось открыть страницу покупки билета");
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
    }

    public class FlightViewModel : BaseViewModel
    {
        public string Route { get; set; }
        public decimal Price { get; set; }
        public string DateRange { get; set; }
        public string Passengers { get; set; }
        public int TicketsFound { get; set; }
        public string Destination { get; set; }
    }

    public class FlightOptionViewModel : BaseViewModel
    {
        public decimal Price { get; set; }
        public string TravelTime { get; set; }
        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
        public string DepartureAirport { get; set; }
        public string ArrivalAirport { get; set; }
        public string Transfers { get; set; }
        public string TransferAirports { get; set; }
        public decimal BaggagePrice { get; set; }
        public string BuyButtonText { get; set; }
        public string Destination { get; set; }
    }
} 