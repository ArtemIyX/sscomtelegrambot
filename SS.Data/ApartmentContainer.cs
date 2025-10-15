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
    public IEnumerable<ApartmentModel> Filter(ApartmentFilter filter) => Map.Values.Where(x => x.MatchesFilter(filter));
}