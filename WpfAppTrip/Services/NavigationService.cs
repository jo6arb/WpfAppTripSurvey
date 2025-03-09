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
            var mainWindow = MainWindow.Instance;
            var frame = mainWindow.FindName("MainFrame") as Frame;
            
            if (frame == null) return;

            switch (pageName)
            {
                case "Survey":
                    frame.Navigate(new SurveyPage());
                    break;
                case "Admin":
                    frame.Navigate(new AdminPage());
                    break;
                case "Register":
                    frame.Navigate(new RegisterPage());
                    break;
                case "Results":
                    // TODO: Добавить страницу результатов
                    break;
            }
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