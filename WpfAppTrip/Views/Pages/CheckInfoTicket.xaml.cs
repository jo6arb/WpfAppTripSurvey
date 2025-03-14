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
using WpfAppTrip.ViewModels;

namespace WpfAppTrip.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для CheckInfoTicket.xaml
    /// </summary>
    public partial class CheckInfoTicket : Page
    {
        public CheckInfoTicket()
        {
            InitializeComponent();
        }

        private void OnSbpSelected(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CheckInfoTicketViewModel viewModel)
            {
                viewModel.IsSbpSelected = true;
                viewModel.IsCardSelected = false;
            }
        }

        private void OnCardSelected(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CheckInfoTicketViewModel viewModel)
            {
                viewModel.IsSbpSelected = false;
                viewModel.IsCardSelected = true;
            }
        }
    }
}
