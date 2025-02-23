using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.Views.Pages;

namespace WpfAppTrip.Views.Controls
{
    public partial class WelcomeControl : UserControl
    {
        public WelcomeControl()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = Window.GetWindow(this)?.FindName("MainFrame") as Frame;
            frame?.Navigate(new Login());
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var frame = Window.GetWindow(this)?.FindName("MainFrame") as Frame;
            frame?.Navigate(new RegisterPage());
        }
    }
} 