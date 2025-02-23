using System;
using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.ViewModels;
using Unity;

namespace WpfAppTrip.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow _instance;
        private readonly MainViewModel _viewModel;
        
        public static MainWindow Instance
        {
            get
            {
                if (_instance == null || !_instance.IsLoaded)
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
            DataContext = _viewModel;

            // Показываем приветственную страницу по умолчанию
            _viewModel.IsWelcomePageVisible = true;
            
            // Обновляем все свойства
            _viewModel.UpdateAllProperties();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _instance = null;
        }

        private void StartSurveyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.NavigateToSurveyCommand.Execute(null);
            }
        }
    }
}
