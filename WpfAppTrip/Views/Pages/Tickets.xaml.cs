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
    /// Логика взаимодействия для Tickets.xaml
    /// </summary>
    public partial class Tickets : Page
    {
        public Tickets(Tour selectedTour)
        {
            InitializeComponent();
            
            // Получаем зависимости из контейнера Unity
            var navigationService = (INavigationService)App.Container.Resolve(typeof(INavigationService));
            var dialogService = (IDialogService)App.Container.Resolve(typeof(IDialogService));
            
            // Создаем экземпляр ViewModel с выбранным туром
            DataContext = new TicketsViewModel(navigationService, dialogService, selectedTour);
        }
    }
}
