using System;
using System.Windows;
using WpfAppTrip.ViewModels;
using Unity;
using WpfAppTrip.Views.Pages;
using WpfAppTrip.Services;

namespace WpfAppTrip.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static LoginWindow _instance;
        
        public static LoginWindow Instance
        {
            get
            {
                if (_instance == null || !_instance.IsLoaded)
                {
                    _instance = new LoginWindow();
                }
                return _instance;
            }
        }

        public LoginWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _instance = null;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Login());
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RegisterPage());
        }
    }
}
