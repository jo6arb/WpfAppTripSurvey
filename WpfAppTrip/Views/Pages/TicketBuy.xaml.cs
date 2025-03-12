using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAppTrip.Models;
using WpfAppTrip.ViewModels;
using WpfAppTrip.Services;
using Unity;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TicketBuy.xaml
    /// </summary>
    public partial class TicketBuy : Page
    {
        public TicketBuy(FlightOptionViewModel selectedFlight)
        {
            InitializeComponent();
            
            // Получаем зависимости из контейнера Unity
            var navigationService = (INavigationService)App.Container.Resolve(typeof(INavigationService));
            var dialogService = (IDialogService)App.Container.Resolve(typeof(IDialogService));
            
            // Создаем экземпляр ViewModel с выбранным рейсом
            DataContext = new TicketBuyViewModel(navigationService, dialogService, selectedFlight);
        }
    }
}
