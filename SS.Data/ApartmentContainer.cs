using System.Collections.Concurrent;

namespace SS.Data;

public class ApartmentContainer
{
    public ConcurrentDictionary<string, ApartmentModel> Map { get; private set; } =
        new ConcurrentDictionary<string, ApartmentModel>();

    public ApartmentModel? Get(string id) => Map.GetValueOrDefault(id);

    public bool Contains(string id) => Map.ContainsKey(id);

    public void Add(string id, ApartmentModel apartment)
    {
        Map[id] = apartment;
    }

    public List<ApartmentModel> GetAll() => Map.Values.ToList();
    public IDictionary<string, IOrderedEnumerable<ApartmentModel>> Filter(ApartmentFilter filter) =>
        Map.Values
            .Where(x => x.MatchesFilter(filter))
            .GroupBy(x => x.Region ?? "Unknown")  // Handle null regions gracefully; adjust if Region can't be null
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)  // Optional: sort groups by region name
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.PricePerMonth)  // Sort each group's apartments by price ascending
            );
}