using System.Windows;
using Unity;
using WpfAppTrip.Services;
using WpfAppTrip.ViewModels;
using WpfAppTrip.Views.Windows;
using WpfAppTrip.Helpers;
using WpfAppTrip.Db;

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

            // Регистрация ViewModels
            container.RegisterType<LoginViewModel>();
            container.RegisterType<RegisterViewModel>();
            container.RegisterType<MainViewModel>();
            container.RegisterType<SurveyViewModel>();

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
