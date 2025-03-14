using System;
using System.Windows.Input;
using WpfAppTrip.Commands;
using WpfAppTrip.Models;
using WpfAppTrip.Services;
using System.Diagnostics;

namespace WpfAppTrip.ViewModels
{
    public class CheckInfoTicketViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly AuthService _authService;
        private Tour _tour;
        private string _routeInfo;
        private string _classInfo;
        private string _totalPriceFormatted;
        private string _passengerFullName;
        private string _passengerBirthDate;
        private string _passengerDocumentInfo;
        private string _passengerDocumentExpiry;
        private string _buyerFullName;
        private string _buyerBirthDate;
        private string _buyerDocumentInfo;
        private string _buyerDocumentExpiry;
        private bool _isSbpSelected;
        private bool _isCardSelected;

        public ICommand BackCommand { get; }
        public ICommand PayCommand { get; }

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

        public string TotalPriceFormatted
        {
            get => _totalPriceFormatted;
            set => SetProperty(ref _totalPriceFormatted, value);
        }

        public string PassengerFullName
        {
            get => _passengerFullName;
            set => SetProperty(ref _passengerFullName, value);
        }

        public string PassengerBirthDate
        {
            get => _passengerBirthDate;
            set => SetProperty(ref _passengerBirthDate, value);
        }

        public string PassengerDocumentInfo
        {
            get => _passengerDocumentInfo;
            set => SetProperty(ref _passengerDocumentInfo, value);
        }

        public string PassengerDocumentExpiry
        {
            get => _passengerDocumentExpiry;
            set => SetProperty(ref _passengerDocumentExpiry, value);
        }

        public string BuyerFullName
        {
            get => _buyerFullName;
            set => SetProperty(ref _buyerFullName, value);
        }

        public string BuyerBirthDate
        {
            get => _buyerBirthDate;
            set => SetProperty(ref _buyerBirthDate, value);
        }

        public string BuyerDocumentInfo
        {
            get => _buyerDocumentInfo;
            set => SetProperty(ref _buyerDocumentInfo, value);
        }

        public string BuyerDocumentExpiry
        {
            get => _buyerDocumentExpiry;
            set => SetProperty(ref _buyerDocumentExpiry, value);
        }

        public bool IsSbpSelected
        {
            get => _isSbpSelected;
            set
            {
                if (SetProperty(ref _isSbpSelected, value) && value)
                {
                    IsCardSelected = false;
                }
            }
        }

        public bool IsCardSelected
        {
            get => _isCardSelected;
            set
            {
                if (SetProperty(ref _isCardSelected, value) && value)
                {
                    IsSbpSelected = false;
                }
            }
        }

        public Tour Tour
        {
            get => _tour;
            set
            {
                if (SetProperty(ref _tour, value))
                {
                    UpdateTourInfo();
                }
            }
        }

        public CheckInfoTicketViewModel(INavigationService navigationService, IDialogService dialogService, AuthService authService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _authService = authService;

            BackCommand = new RelayCommand(OnBack);
            PayCommand = new RelayCommand(OnPay);

            // Установка значений по умолчанию
            IsSbpSelected = true;
            IsCardSelected = false;

            // Заполнение тестовыми данными
            RouteInfo = "Москва - Пхукет";
            ClassInfo = "8 июль, 1 класс Эконом";
            TotalPriceFormatted = "185.282₽";
            PassengerFullName = "MAXIMOVA MARINA";
            PassengerBirthDate = "29.07.2005";
            PassengerDocumentInfo = "Загранпаспорт РФ 48 7109853";
            PassengerDocumentExpiry = "25.12.2028";
            BuyerFullName = "МАКСИМОВА МАРИНА";
            BuyerBirthDate = "29.07.2005";
            BuyerDocumentInfo = "Загранпаспорт РФ 48 7109853";
            BuyerDocumentExpiry = "25.12.2028";
        }

        private void UpdateTourInfo()
        {
            if (_tour != null)
            {
                RouteInfo = $"Москва - {_tour.Destination ?? "Пхукет"}";
                TotalPriceFormatted = $"{_tour.Price:N0}₽";
            }
        }

        private void OnBack(object parameter)
        {
            _navigationService.GoBack();
        }

        private void OnPay(object parameter)
        {
            try
            {
                if (!IsSbpSelected && !IsCardSelected)
                {
                    _dialogService.ShowWarning("Пожалуйста, выберите способ оплаты");
                    return;
                }

                // В реальном приложении здесь был бы код для обработки платежа
                _dialogService.ShowInfo("Билет успешно оплачен! Информация о билете отправлена на вашу электронную почту.");

                // Переход на страницу успешной оплаты или главную страницу
                _navigationService.NavigateToWelcome();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при оплате: {ex.Message}");
                _dialogService.ShowError($"Ошибка при оплате: {ex.Message}");
            }
        }

        public void Initialize(Tour tour, string passengerName, string passengerBirthDate, string documentInfo, string documentExpiry)
        {
            Tour = tour;
            PassengerFullName = passengerName.ToUpper();
            PassengerBirthDate = passengerBirthDate;
            PassengerDocumentInfo = documentInfo;
            PassengerDocumentExpiry = documentExpiry;

            // Если пользователь авторизован, используем его данные для покупателя
            if (_authService.IsAuthenticated && _authService.CurrentUser != null)
            {
                BuyerFullName = _authService.CurrentUser.Username.ToUpper();
                // Другие данные покупателя можно заполнить из профиля пользователя
            }
            else
            {
                // Если пользователь не авторизован, используем данные пассажира
                BuyerFullName = passengerName.ToUpper();
                BuyerBirthDate = passengerBirthDate;
                BuyerDocumentInfo = documentInfo;
                BuyerDocumentExpiry = documentExpiry;
            }
        }
    }
} 