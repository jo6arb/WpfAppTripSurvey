using System.Windows;

namespace WpfAppTrip.Services
{
    public class DialogService : IDialogService
    {
        public void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowConfirm(string message)
        {
            return MessageBox.Show(message, "Подтверждение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
} 