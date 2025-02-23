using System.Windows.Controls;
using System.Windows;
using WpfAppTrip.Views.Pages;
using WpfAppTrip.Views.Windows;

namespace WpfAppTrip.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;
        private Window _currentWindow;

        public NavigationService(Frame frame = null)
        {
            _frame = frame;
        }

        public void GoBack()
        {
            if (_frame?.CanGoBack == true)
                _frame.GoBack();
        }

        public void NavigateToPage(string pageName)
        {
            if (_frame == null) return;

            Page page = null;
            switch (pageName)
            {
                case "Register":
                    page = new RegisterPage();
                    break;
                case "Survey":
                    page = new SurveyPage();
                    break;
                case "Admin":
                    page = new AdminPage();
                    break;
            }

            if (page != null)
                _frame.Navigate(page);
        }

        public void ShowLoginWindow()
        {
            var loginWindow = LoginWindow.Instance;
            loginWindow.Show();
            
            if (_currentWindow != null && _currentWindow != loginWindow)
            {
                _currentWindow.Close();
            }
            _currentWindow = loginWindow;
        }

        public void ShowMainWindow()
        {
            var mainWindow = MainWindow.Instance;
            mainWindow.Show();
            
            if (_currentWindow != null && _currentWindow != mainWindow)
            {
                _currentWindow.Close();
            }
            _currentWindow = mainWindow;
        }
    }
} 