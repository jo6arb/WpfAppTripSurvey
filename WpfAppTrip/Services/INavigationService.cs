using System;

namespace WpfAppTrip.Services
{
    /// <summary>
    /// Интерфейс сервиса навигации для перемещения между страницами и окнами приложения
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Возвращается на предыдущую страницу, если возможно
        /// </summary>
        void GoBack();
        
        /// <summary>
        /// Переходит к указанной странице
        /// </summary>
        /// <param name="pageName">Имя страницы для навигации</param>
        void NavigateToPage(string pageName);
        
        /// <summary>
        /// Переходит к указанной странице с передачей параметра
        /// </summary>
        /// <param name="pageName">Имя страницы для навигации</param>
        /// <param name="parameter">Параметр для передачи на страницу</param>
        void NavigateToPage(string pageName, object parameter);
        
        /// <summary>
        /// Показывает главное окно
        /// </summary>
        void ShowMainWindow();
        
        /// <summary>
        /// Переходит к странице приветствия
        /// </summary>
        void NavigateToWelcome();
        
        /// <summary>
        /// Обновляет текущую страницу
        /// </summary>
        void RefreshCurrentPage();
        
        /// <summary>
        /// Показывает окно входа
        /// </summary>
        void ShowLoginWindow();
        
        /// <summary>
        /// Переходит к странице туров
        /// </summary>
        void NavigateToTours();
        
        /// <summary>
        /// Событие, возникающее при изменении навигации
        /// </summary>
        event EventHandler<NavigationEventArgs> Navigated;
    }
} 