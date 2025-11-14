namespace SS.Notifier.Data.Settings;

// Configuration model classes

public class AppSettings
{
    public class ApartmentFilter
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinArea { get; set; }
        public decimal MaxArea { get; set; }
        public List<int> Rooms { get; set; } = new List<int>();
    }

    public class TelegramSettings
    {
        public long Chat { get; set; } = 0;
        public Dictionary<string, int> Threads { get; set; } = new Dictionary<string, int>();
        public List<string> Regions => Threads.Keys.ToHashSet().ToList();
    }

    public AppSettings.TelegramSettings Telegram { get; set; } = new TelegramSettings();
    public AppSettings.ApartmentFilter Filter { get; set; } = new ApartmentFilter();
}