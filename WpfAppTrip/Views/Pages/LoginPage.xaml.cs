using System.Windows;
using System.Windows.Controls;
using WpfAppTrip.ViewModels;

namespace WpfAppTrip.Views.Pages
{
    public partial class LoginPage : Page
    {
        private LoginViewModel _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = (LoginViewModel)DataContext;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Очистка полей при загрузке страницы
            PhoneTextBox.Text = "+7";
            PasswordBox.Clear();
            
            ValidateFields();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = PasswordBox.Password;
                ValidateFields();
            }
        }

        private void ValidateFields()
        {
            if (_viewModel != null)
            {
                _viewModel.Phone = PhoneTextBox.Text;
                _viewModel.Password = PasswordBox.Password;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = PasswordBox.Password;
            }
        }
    }
} 