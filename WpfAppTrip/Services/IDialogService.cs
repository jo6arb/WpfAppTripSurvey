namespace WpfAppTrip.Services
{
    public interface IDialogService
    {
        void ShowError(string message);
        void ShowWarning(string message);
        void ShowInfo(string message);
        bool ShowConfirm(string message);
        bool ShowQuestion(string message, string title);
    }
} 