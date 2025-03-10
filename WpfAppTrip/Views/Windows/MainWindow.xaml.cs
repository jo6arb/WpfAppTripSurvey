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

            // При первом запуске показываем страницу приветствия
            if (MainFrame != null)
            {
                MainFrame.Navigate(new WelcomePage());
            }
            
            // Обновляем все свойства
            _viewModel.UpdateAllProperties();

            // Регистрируем обработчик закрытия окна
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }

        private void StartSurveyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.NavigateToSurveyCommand.Execute(null);
            }
        }

        // Вспомогательный метод для получения MainFrame из других классов
        public Frame GetMainFrame()
        {
            return MainFrame;
        }
    }
}
