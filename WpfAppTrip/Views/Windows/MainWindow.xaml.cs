using System;
using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.ViewModels;
using Unity;
using WpfAppTrip.Services;
using WpfAppTrip.Views.Pages;

namespace WpfAppTrip.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow _instance;
        private readonly MainViewModel _viewModel;
        private readonly INavigationService _navigationService;
        
        // Флаг, указывающий, что окно закрывается из-за навигации к другому окну
        public bool IsClosingForNavigation { get; set; }
        
        public static MainWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainWindow();
                }
                return _instance;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = App.Container.Resolve<MainViewModel>();
            _navigationService = App.Container.Resolve<INavigationService>();
            DataContext = _viewModel;

            // Navigate to WelcomePage on startup
            MainFrame.Navigate(new WelcomePage());

            // Update all properties
            _viewModel.UpdateAllProperties();

            // Register event handler for window close
            Closed += MainWindow_Closed;

            // Subscribe to the event to navigate to the welcome page
            _viewModel.NavigateToWelcomePage += OnNavigateToWelcomePage;
            
            // Subscribe to the event to navigate to the profile page
            _viewModel.NavigateToProfilePage += OnNavigateToProfilePage;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // Если окно закрывается из-за навигации, не завершаем приложение
            if (!IsClosingForNavigation)
            {
                Application.Current.Shutdown();
            }
        }

        private void StartSurveyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.NavigateToSurveyCommand.Execute(null);
            }
        }

        private void OnNavigateToWelcomePage()
        {
            MainFrame.Navigate(new WelcomePage());
        }

        private void OnNavigateToProfilePage()
        {
            ProfileFrame.Navigate(new CabinetExt());
        }

        // Вспомогательный метод для получения MainFrame из других классов
        public Frame GetMainFrame()
        {
            return MainFrame;
        }
    }
}
