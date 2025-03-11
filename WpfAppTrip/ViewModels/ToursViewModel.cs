using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WpfAppTrip.Models;
using WpfAppTrip.Services;
using WpfAppTrip.Db;
using System.Diagnostics;
using System.Windows;

namespace WpfAppTrip.ViewModels
{
    public class ToursViewModel : BaseViewModel
    {
        private ObservableCollection<Tour> _tours;
        private readonly INavigationService _navigationService;
        private readonly Dbhelper _db;
        private bool _isLoading;
        
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

        public ToursViewModel(INavigationService navigationService)
        {
            Debug.WriteLine("ToursViewModel: Конструктор начал работу");
            _navigationService = navigationService;
            _db = new Dbhelper();
            Tours = new ObservableCollection<Tour>();
            Debug.WriteLine("ToursViewModel: Вызываем LoadToursAsync");
            LoadToursAsync();
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