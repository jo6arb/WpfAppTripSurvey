using System.Windows.Controls;
using Unity;
using WpfAppTrip.ViewModels;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Tours.xaml
    /// </summary>
    public partial class Tours : Page
    {
        public Tours()
        {
            InitializeComponent();
            DataContext = App.Container.Resolve<ToursViewModel>();
        }
    }
}
