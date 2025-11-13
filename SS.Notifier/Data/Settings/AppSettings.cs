namespace SS.Notifier.Data.Settings;

// Configuration model classes
public class AppSettings
{
    public Dictionary<string, long> Chats { get; set; } = new();
}