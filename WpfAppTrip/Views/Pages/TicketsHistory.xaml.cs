using System.Windows.Controls;
using WpfAppTrip.ViewModels;
using WpfAppTrip.Services;
using WpfAppTrip.Db;
using Unity;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TicketsHistory.xaml
    /// </summary>
    public partial class TicketsHistory : Page
    {
        public TicketsHistory()
        {
            InitializeComponent();
            
            // Получаем зависимости из контейнера Unity
            var navigationService = (INavigationService)App.Container.Resolve(typeof(INavigationService));
            var dialogService = (IDialogService)App.Container.Resolve(typeof(IDialogService));
            var dbHelper = (Dbhelper)App.Container.Resolve(typeof(Dbhelper));
            
            // Создаем экземпляр ViewModel с необходимыми зависимостями
            DataContext = new TicketsHistoryViewModel(navigationService, dialogService, dbHelper);
        }
    }
} 