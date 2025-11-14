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
        public List<int> Rooms { get; set; }
    }

    public Dictionary<string, long> Chats { get; set; } = new();

    public List<string> Regions => Chats.Keys.ToHashSet().ToList();

    public AppSettings.ApartmentFilter Filter { get; set; } = new ApartmentFilter();
}