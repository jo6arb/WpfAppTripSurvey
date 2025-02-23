namespace WpfAppTrip.Services
{
    public interface INavigationService
    {
        void GoBack();
        void NavigateToPage(string pageName);
        void ShowLoginWindow();
        void ShowMainWindow();
    }
} 