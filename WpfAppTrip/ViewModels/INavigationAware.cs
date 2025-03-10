namespace WpfAppTrip.ViewModels
{
    /// <summary>
    /// Интерфейс для ViewModel, которые должны реагировать на навигацию
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>
        /// Вызывается при навигации к странице
        /// </summary>
        /// <param name="parameter">Параметр навигации</param>
        void OnNavigatedTo(object parameter);
        
        /// <summary>
        /// Вызывается при навигации от страницы
        /// </summary>
        void OnNavigatedFrom();
    }
} 