using System.Windows;
using Unity;
using WpfAppTrip.Services;
using WpfAppTrip.ViewModels;
using WpfAppTrip.Views.Windows;
using WpfAppTrip.Helpers;
using WpfAppTrip.Db;
using Unity.Injection;

namespace WpfAppTrip
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static IUnityContainer _container;

        public App()
        {
            _container = ConfigureContainer();
        }

        // Сделаем свойство публичным и статическим
        public static IUnityContainer Container { get; private set; }

        private static IUnityContainer ConfigureContainer()
        {
            var container = new UnityContainer();

            // Регистрация сервисов
            container.RegisterSingleton<IDialogService, DialogService>();
            container.RegisterSingleton<INavigationService, NavigationService>();
            container.RegisterSingleton<AuthService>();
            container.RegisterSingleton<Dbhelper>();
            container.RegisterSingleton<IPdfTicketService, PdfTicketService>();

            // Регистрация ViewModels
            container.RegisterType<LoginViewModel>();
            container.RegisterType<SurveyViewModel>();
            container.RegisterType<ToursViewModel>();
            container.RegisterType<ProfileViewModel>(new InjectionConstructor(
                new ResolvedParameter<AuthService>(),
                new ResolvedParameter<IDialogService>(),
                new ResolvedParameter<INavigationService>(),
                new ResolvedParameter<Dbhelper>()
            ));
            container.RegisterType<TicketsHistoryViewModel>();

            // Регистрация TicketBuyViewModel
            container.RegisterType<TicketBuyViewModel>(new InjectionConstructor(
                new ResolvedParameter<INavigationService>(),
                new ResolvedParameter<IDialogService>(),
                new ResolvedParameter<IPdfTicketService>(),
                new ResolvedParameter<Dbhelper>(),
                new ResolvedParameter<FlightOptionViewModel>()
            ));

            // Сохраняем контейнер в статическом свойстве
            Container = container;
            return container;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Копируем изображения из папки проекта в bin/Debug
            ImagePathHelper.CopyImagesToOutput();
            
            var loginWindow = LoginWindow.Instance;
            loginWindow.Show();
        }
    }
}
