using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.Services;
using Unity;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        private readonly INavigationService _navigationService;

        public WelcomePage()
        {
            InitializeComponent();
            _navigationService = App.Container.Resolve<INavigationService>();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToPage("Login");
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _navigationService.NavigateToPage("Register");
        }
    }
} 