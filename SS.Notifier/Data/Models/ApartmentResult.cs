using SS.Notifier.Data.Entity;

namespace SS.Notifier.Data.Models;

public class ApartmentResult
{
    public Dictionary<string, List<ApartmentEntity>> Container { get; set; } =
        new Dictionary<string, List<ApartmentEntity>>();

    /// <summary>
    /// Groups a flat list of ApartmentEntity objects by their Region property.
    /// </summary>
    /// <param name="list">The source list (may be null or empty).</param>
    public ApartmentResult(List<ApartmentEntity> list)
    {
        if (list.Count == 0)
            return; // Container stays empty

        // LINQ GroupBy -> Dictionary
        Container = list
            .GroupBy(a => a.Region ?? string.Empty) // treat null Region as empty string
            .ToDictionary(
                g => g.Key,
                g => g.ToList()
            );
    }

    /// <summary>
    /// Returns all region names currently in the container.
    /// </summary>
    public IReadOnlyCollection<string> Regions
        => Container.Keys;


    /// <summary>
    /// Indexer: Get list of apartments by region.
    /// Returns null if region doesn't exist.
    /// </summary>
    /// <param name="region">The region name (case-insensitive if comparer is set).</param>
    public List<ApartmentEntity>? this[string? region]
    {
        get
        {
            if (string.IsNullOrEmpty(region))
                return Container.GetValueOrDefault(string.Empty);

            Container.TryGetValue(region, out var list);
            return list;
        }
    }
}