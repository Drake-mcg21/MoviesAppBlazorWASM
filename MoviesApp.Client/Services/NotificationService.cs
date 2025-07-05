namespace MoviesApp.Client.Services
{
    public enum NotificationLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class NotificationService
    {
        public event Action<string, NotificationLevel>? OnNotify;

        public void Show(string message, NotificationLevel level = NotificationLevel.Info)
        {
            OnNotify?.Invoke(message, level);
        }
    }
}
