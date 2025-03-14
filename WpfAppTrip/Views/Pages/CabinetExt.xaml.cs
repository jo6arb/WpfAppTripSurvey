using System.Windows.Controls;
using WpfAppTrip.ViewModels;
using WpfAppTrip.Services;
using Unity;
using WpfAppTrip.Db;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для CabinetExt.xaml
    /// </summary>
    public partial class CabinetExt : Page
    {
        public CabinetExt()
        {
            InitializeComponent();
            
            // Получаем зависимости из контейнера Unity
            var authService = (AuthService)App.Container.Resolve(typeof(AuthService));
            var dialogService = (IDialogService)App.Container.Resolve(typeof(IDialogService));
            var navigationService = (INavigationService)App.Container.Resolve(typeof(INavigationService));
            var db = (Dbhelper)App.Container.Resolve(typeof(Dbhelper));
            
            // Создаем экземпляр ProfileViewModel вручную
            DataContext = new ProfileViewModel(authService, dialogService, navigationService, db);
        }
    }
}
