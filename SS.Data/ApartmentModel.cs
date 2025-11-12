using System.Globalization;

namespace SS.Data;

public class ApartmentModel : IEquatable<ApartmentModel>
{
    public string Region { get; }
    public string Rooms { get; }
    public string Area { get; }
    public string Floor { get; }
    public string Series { get; }
    public decimal PricePerMonth { get; }
    public string Link { get; }

    public ApartmentModel(string region, string rooms, string area, string floor, string series, decimal pricePerMonth,
        string link)
    {
        Region = region;
        Rooms = rooms;
        Area = area;
        Floor = floor;
        Series = series;
        PricePerMonth = pricePerMonth;
        Link = link;
    }

    public bool MatchesFilter(ApartmentFilter filter)
    {
        // Check price

        if (PricePerMonth < filter.MinPrice)
            return false;

        if (filter.MaxPrice.HasValue && PricePerMonth > filter.MaxPrice.Value)
            return false;


        // Check rooms - if filter has room restrictions, apartment must match one of them
        if (filter.Rooms != null && filter.Rooms.Count > 0)
        {
            if (int.TryParse(Rooms, out int roomCount))
            {
                if (!filter.Rooms.Contains(roomCount))
                    return false;
            }
        }

        // Check area/square
        if (decimal.TryParse(Area, out decimal areaValue))
        {
            if (areaValue < filter.MinSquare)
                return false;

            if (filter.MaxSquare.HasValue && areaValue > filter.MaxSquare.Value)
                return false;
        }

        // Check regions - if filter has region restrictions, apartment must match one of them
        if (filter.Regions != null && filter.Regions.Count > 0)
        {
            if (!filter.Regions.Contains(Region, StringComparer.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    public decimal ParseArea() => decimal.Parse(Area, CultureInfo.InvariantCulture);

    public decimal PricePerSquare()
    {
        if (decimal.TryParse(Area, CultureInfo.InvariantCulture, out decimal areaValue))
        {
            if (PricePerMonth != 0.0m)
            {
                return PricePerMonth / areaValue;
            }
        }

        return 0.0m;
    }

    public int ParseRooms() => int.Parse(Rooms, CultureInfo.InvariantCulture);

    public string Id => Link.Split('/', StringSplitOptions.RemoveEmptyEntries)
        .LastOrDefault()?
        .Replace(".html", "") ?? string.Empty;


    public override string ToString()
    {
        return $"[{Region}] {PricePerMonth}eur {Rooms}r {Area}m2 {Floor} {Series} {Link}";
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Region);
        hash.Add(Rooms);
        hash.Add(Area);
        hash.Add(Floor);
        hash.Add(Series);
        hash.Add(PricePerMonth);
        hash.Add(Link);
        return hash.ToHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ApartmentModel);
    }

    public bool Equals(ApartmentModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Region == other.Region &&
               Rooms == other.Rooms &&
               Area == other.Area &&
               Floor == other.Floor &&
               Series == other.Series &&
               PricePerMonth == other.PricePerMonth &&
               Link == other.Link;
    }

    public int ParseFloor()
    {
        if (string.IsNullOrWhiteSpace(Floor))
            return 0;

        var parts = Floor.Split('/');
        if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int floor))
            return floor;

        return 0;
    }

    public int ParseMaxFloor()
    {
        if (string.IsNullOrWhiteSpace(Floor))
            return 0;

        var parts = Floor.Split('/');
        if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int maxFloor))
            return maxFloor;

        return 0;
    }
}