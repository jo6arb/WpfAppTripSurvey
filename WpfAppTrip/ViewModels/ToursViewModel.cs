using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using WpfAppTrip.Models;
using WpfAppTrip.Services;
using WpfAppTrip.Db;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WpfAppTrip.Commands;
using System.ComponentModel;

namespace WpfAppTrip.ViewModels
{
    public class ToursViewModel : BaseViewModel
    {
        private ObservableCollection<Tour> _tours;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly AuthService _authService;
        private readonly Dbhelper _db;
        private bool _isLoading;
        private Tour _selectedTour;
        private readonly Dictionary<int, bool> _selectedTours;
        
        public ICommand SelectTourCommand { get; }
        public ICommand BookTourCommand { get; }

        public ObservableCollection<Tour> Tours
        {
            get => _tours;
            set => SetProperty(ref _tours, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set 
            { 
                Debug.WriteLine($"IsLoading изменено с {_isLoading} на {value}");
                SetProperty(ref _isLoading, value);
            }
        }

        public Tour SelectedTour
        {
            get => _selectedTour;
            set => SetProperty(ref _selectedTour, value);
        }

        public bool IsTourSelected(Tour tour)
        {
            return tour != null && _selectedTours.ContainsKey(tour.TourID) && _selectedTours[tour.TourID];
        }

        public ToursViewModel(INavigationService navigationService, IDialogService dialogService, AuthService authService)
        {
            Debug.WriteLine("ToursViewModel: Конструктор начал работу");
            _navigationService = navigationService;
            _dialogService = dialogService;
            _authService = authService;
            _db = new Dbhelper();
            Tours = new ObservableCollection<Tour>();
            _selectedTours = new Dictionary<int, bool>();

            SelectTourCommand = new RelayCommand(OnSelectTour);
            BookTourCommand = new RelayCommand(OnBookTour);

            Debug.WriteLine("ToursViewModel: Вызываем LoadToursAsync");
            LoadToursAsync();

            PropertyChanged += ToursViewModel_PropertyChanged;
        }

        private void ToursViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedTour))
            {
                OnPropertyChanged(nameof(IsTourSelected));
            }
        }

        private void OnSelectTour(object parameter)
        {
            if (parameter is Tour tour)
            {
                // Сбрасываем выделение у всех туров
                foreach (var tourId in _selectedTours.Keys.ToList())
                {
                    _selectedTours[tourId] = false;
                }
                
                // Выделяем выбранный тур
                _selectedTours[tour.TourID] = true;
                SelectedTour = tour;
                
                // Уведомляем об изменении для всех туров
                OnPropertyChanged(nameof(IsTourSelected));
            }
        }

        private async void OnBookTour(object parameter)
        {
            if (parameter is Tour tour)
            {
                if (!_authService.IsAuthenticated)
                {
                    _dialogService.ShowWarning("Для бронирования тура необходимо войти в систему");
                    _navigationService.NavigateToPage("Login");
                    return;
                }

                try
                {
                    string insertQuery = @"
                        INSERT INTO Bookings (UserID, TourID, BookingDate, TravelDate, NumberOfPeople, TotalPrice, Status)
                        VALUES (@UserID, @TourID, GETDATE(), DATEADD(month, 1, GETDATE()), 1, @Price, N'Ожидание')";

                    var parameters = new Dictionary<string, object>
                    {
                        { "@UserID", _authService.CurrentUser.UserID },
                        { "@TourID", tour.TourID },
                        { "@Price", tour.Price }
                    };

                    await _db.ExecuteNonQueryAsync(insertQuery, parameters);
                    _dialogService.ShowInfo("Тур успешно забронирован! Мы свяжемся с вами для уточнения деталей.");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowError($"Ошибка при бронировании тура: {ex.Message}");
                }
            }
        }

        private async void LoadToursAsync()
        {
            try
            {
                Debug.WriteLine("LoadToursAsync: Начало загрузки туров");
                IsLoading = true;

                // Добавляем задержку в 5 секунд
                await Task.Delay(5000);

                string query = @"SELECT t.TourID, t.Name, t.Description, t.Price, t.Duration, t.ImagePath, 
                             t.Season, t.Difficulty, t.MaxGroupSize, tc.CategoryName
                      FROM Tours t
                      LEFT JOIN TourCategories tc ON t.CategoryID = tc.CategoryID
                      WHERE t.IsActive = 1
                      ORDER BY t.TourID";

                Debug.WriteLine($"LoadToursAsync: Выполняем запрос: {query}");

                var tours = await _db.GetDataListAsync<Tour>(query,
                    reader =>
                    {
                        try
                        {
                            var tour = new Tour
                            {
                                TourID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                Duration = reader.GetInt32(4),
                                ImagePath = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Season = reader.GetString(6),
                                Difficulty = reader.GetString(7),
                                MaxGroupSize = reader.GetInt32(8)
                            };
                            Debug.WriteLine($"LoadToursAsync: Загружен тур: ID={tour.TourID}, Name={tour.Name}");
                            return tour;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"LoadToursAsync: Ошибка при чтении данных тура: {ex.Message}");
                            throw;
                        }
                    }
                );

                Debug.WriteLine($"LoadToursAsync: Загружено туров: {tours?.Count ?? 0}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        Tours.Clear();
                        if (tours != null)
                        {
                            foreach (var tour in tours)
                            {
                                Tours.Add(tour);
                                Debug.WriteLine($"LoadToursAsync: Добавлен тур в коллекцию: {tour.Name}");
                            }
                        }
                        Debug.WriteLine($"LoadToursAsync: Всего туров в коллекции: {Tours.Count}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"LoadToursAsync: Ошибка при обновлении коллекции: {ex.Message}");
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadToursAsync: Критическая ошибка: {ex.Message}");
                Debug.WriteLine($"LoadToursAsync: StackTrace: {ex.StackTrace}");
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var errorMessage = $"Ошибка при загрузке туров:\n{ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nДетали ошибки:\n{ex.InnerException.Message}";
                    }
                    MessageBox.Show(errorMessage, "Ошибка загрузки туров",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("LoadToursAsync: Загрузка завершена");
            }
        }
    }
} 