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
}