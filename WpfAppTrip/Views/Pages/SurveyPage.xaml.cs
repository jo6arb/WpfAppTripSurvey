using System.Windows;
using System.Windows.Controls;
using Unity;
using WpfAppTrip.ViewModels;

namespace WpfAppTrip.Views.Pages
{
    public partial class SurveyPage : Page
    {
        public SurveyPage()
        {
            InitializeComponent();
            DataContext = App.Container.Resolve<SurveyViewModel>();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && DataContext is SurveyViewModel viewModel)
            {
                int optionId = (int)radioButton.Tag;
                viewModel.SelectAnswer(optionId);
            }
        }
    }
} 