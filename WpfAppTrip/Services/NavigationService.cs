using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfAppTrip.Views.Pages;
using WpfAppTrip.Views.Windows;
using WpfAppTrip.Views.Controls;
using WpfAppTrip.ViewModels;
using WpfAppTrip.Services;
using Unity;

namespace WpfAppTrip.Services
{
    /// <summary>
    /// Сервис навигации для перемещения между страницами и окнами приложения
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Type> _pageTypes;
        private Frame _mainFrame;
        private Window _currentWindow;
        private string _currentPageName;

        /// <summary>
        /// Событие, возникающее при изменении навигации
        /// </summary>
        public event EventHandler<NavigationEventArgs> Navigated;

        /// <summary>
        /// Инициализирует новый экземпляр класса NavigationService
        /// </summary>
        public NavigationService()
        {
            // Регистрируем все страницы приложения для навигации
            _pageTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Welcome", typeof(WelcomePage) },
                { "Login", typeof(Login) },
                { "Register", typeof(RegisterPage) },
                { "Survey", typeof(SurveyPage) },
                { "Admin", typeof(AdminPage) },
                { "Tours", typeof(Tours) },
                { "Tickets", typeof(Tickets) },
                { "TicketsHistory", typeof(TicketsHistory) }
            };
            
            Debug.WriteLine("NavigationService: Зарегистрированы страницы:");
            foreach (var page in _pageTypes)
            {
                Debug.WriteLine($"- {page.Key}: {page.Value.Name}");
            }
        }

        /// <summary>
        /// Возвращается на предыдущую страницу, если возможно
        /// </summary>
        public void GoBack()
        {
            try
            {
                if (GetMainFrame()?.CanGoBack == true)
                {
                    GetMainFrame().GoBack();
                    _currentPageName = GetPageNameFromType(GetMainFrame().Content.GetType());
                    OnNavigated(new NavigationEventArgs { PageName = _currentPageName });
                }
                else
                {
                    NavigateToWelcome();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при возврате на предыдущую страницу: {ex.Message}");
            }
        }

        /// <summary>
        /// Переходит к указанной странице
        /// </summary>
        /// <param name="pageName">Имя страницы для навигации</param>
        public void NavigateToPage(string pageName)
        {
            NavigateToPage(pageName, null);
        }

        /// <summary>
        /// Переходит к указанной странице с передачей параметра
        /// </summary>
        /// <param name="pageName">Имя страницы для навигации</param>
        /// <param name="parameter">Параметр для передачи на страницу</param>
        public void NavigateToPage(string pageName, object parameter)
        {
            try
            {
                if (!_pageTypes.TryGetValue(pageName, out var pageType))
                {
                    Debug.WriteLine($"Страница '{pageName}' не зарегистрирована в NavigationService");
                    return;
                }

                var page = Activator.CreateInstance(pageType);
                if (page is Page newPage)
                {
                    // Передаем параметр навигации через DataContext, если страница поддерживает INavigationAware
                    if (parameter != null && newPage.DataContext is INavigationAware navigationAware)
                    {
                        navigationAware.OnNavigatedTo(parameter);
                    }

                    GetMainFrame()?.Navigate(newPage);
                    _currentPageName = pageName;
                    OnNavigated(new NavigationEventArgs { PageName = pageName, Parameter = parameter });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при навигации к странице '{pageName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Показывает окно входа
        /// </summary>
        public void ShowLoginWindow()
        {
            try
            {
                // Создаем новое окно входа
                var loginWindow = LoginWindow.Instance;
                
                // Устанавливаем его как главное окно приложения
                Application.Current.MainWindow = loginWindow;
                
                // Показываем окно входа
                loginWindow.Show();
                
                // Закрываем текущее окно, если оно не является окном входа
                if (_currentWindow != null && _currentWindow != loginWindow)
                {
                    // Отключаем обработчик OnClosed, чтобы избежать вызова Shutdown
                    if (_currentWindow is MainWindow mainWindow)
                    {
                        // Устанавливаем флаг, что окно закрывается из-за перехода к окну входа
                        mainWindow.IsClosingForNavigation = true;
                    }
                    
                    _currentWindow.Close();
                }
                
                // Обновляем текущее окно
                _currentWindow = loginWindow;
                
                // Сбрасываем Frame для следующего окна
                _mainFrame = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при отображении окна входа: {ex.Message}");
            }
        }

        /// <summary>
        /// Показывает главное окно
        /// </summary>
        public void ShowMainWindow()
        {
            try
            {
                // Создаем новое главное окно
                var mainWindow = MainWindow.Instance;
                
                // Устанавливаем его как главное окно приложения
                Application.Current.MainWindow = mainWindow;
                
                // Показываем главное окно
                mainWindow.Show();
                
                // Закрываем текущее окно, если оно не является главным окном
                if (_currentWindow != null && _currentWindow != mainWindow)
                {
                    // Если текущее окно - LoginWindow, просто закрываем его
                    _currentWindow.Close();
                }
                
                // Обновляем текущее окно
                _currentWindow = mainWindow;
                
                // Сбрасываем Frame для следующего окна
                _mainFrame = mainWindow.GetMainFrame();
                
                // Показываем страницу приветствия
                var mainViewModel = (MainViewModel)App.Container.Resolve(typeof(MainViewModel));
                if (mainViewModel != null)
                {
                    mainViewModel.IsWelcomePageVisible = true;
                    mainViewModel.UpdateAllProperties();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при отображении главного окна: {ex.Message}");
            }
        }

        /// <summary>
        /// Переходит к странице приветствия
        /// </summary>
        public void NavigateToWelcome()
        {
            NavigateToPage("Welcome");
        }

        /// <summary>
        /// Переходит к странице регистрации
        /// </summary>
        public void NavigateToRegister()
        {
            NavigateToPage("Register");
        }

        /// <summary>
        /// Переходит к странице опроса
        /// </summary>
        public void NavigateToSurvey()
        {
            NavigateToPage("Survey");
        }

        /// <summary>
        /// Переходит к странице администратора
        /// </summary>
        public void NavigateToAdmin()
        {
            NavigateToPage("Admin");
        }

        /// <summary>
        /// Переходит к странице туров
        /// </summary>
        public void NavigateToTours()
        {
            NavigateToPage("Tours");
        }

        /// <summary>
        /// Переходит к странице бронирования билетов
        /// </summary>
        /// <param name="tour">Выбранный тур для бронирования</param>
        public void NavigateToTickets(WpfAppTrip.Models.Tour tour)
        {
            try
            {
                if (tour == null)
                {
                    Debug.WriteLine("NavigateToTickets: Тур не указан");
                    return;
                }

                var ticketsPage = new Tickets(tour);
                GetMainFrame()?.Navigate(ticketsPage);
                _currentPageName = "Tickets";
                OnNavigated(new NavigationEventArgs { PageName = "Tickets", Parameter = tour });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при навигации к странице бронирования билетов: {ex.Message}");
            }
        }

        /// <summary>
        /// Переход на страницу истории билетов
        /// </summary>
        public void NavigateToTicketsHistory()
        {
            NavigateToPage("TicketsHistory");
        }

        /// <summary>
        /// Получает главный фрейм приложения
        /// </summary>
        /// <returns>Главный фрейм приложения</returns>
        public Frame GetMainFrame()
        {
            if (_mainFrame != null)
                return _mainFrame;

            // Найти главный фрейм в текущем окне
            if (_currentWindow?.FindName("MainFrame") is Frame frame)
            {
                _mainFrame = frame;
                return _mainFrame;
            }

            // Если окно не задано, ищем в MainWindow
            var mainWindow = MainWindow.Instance;
            if (mainWindow?.FindName("MainFrame") is Frame mainFrame)
            {
                _mainFrame = mainFrame;
                return _mainFrame;
            }

            return null;
        }

        /// <summary>
        /// Обновляет текущую страницу
        /// </summary>
        public void RefreshCurrentPage()
        {
            if (!string.IsNullOrEmpty(_currentPageName))
            {
                NavigateToPage(_currentPageName);
            }
        }

        /// <summary>
        /// Получает имя страницы по типу
        /// </summary>
        private string GetPageNameFromType(Type pageType)
        {
            foreach (var pair in _pageTypes)
            {
                if (pair.Value == pageType)
                    return pair.Key;
            }
            return null;
        }

        /// <summary>
        /// Вызывает событие навигации
        /// </summary>
        protected virtual void OnNavigated(NavigationEventArgs e)
        {
            Navigated?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Интерфейс для страниц, которые получают параметры при навигации
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Вызывается, когда происходит навигация к странице
        /// </summary>
        /// <param name="parameter">Параметр навигации</param>
        void OnNavigatedTo(object parameter);

        /// <summary>
        /// Вызывается, когда происходит навигация от страницы
        /// </summary>
        void OnNavigatedFrom();
    }

    /// <summary>
    /// Аргументы события навигации
    /// </summary>
    public class NavigationEventArgs : EventArgs
    {
        /// <summary>
        /// Имя страницы
        /// </summary>
        public string PageName { get; set; }
        
        /// <summary>
        /// Параметр навигации
        /// </summary>
        public object Parameter { get; set; }
    }
} 