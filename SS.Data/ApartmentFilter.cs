using System.Globalization;

namespace SS.Data;

public class ApartmentFilter
{
    public decimal MinPrice { get; set; } = 0;
    
    public decimal? MaxPrice { get; set; } = null;
    
    public List<int>? Rooms { get; set; } = null;
    
    public decimal MinSquare { get; set; } = 0;
    
    public decimal? MaxSquare { get; set; } = null;
    
    public List<string>? Regions { get; set; } = null;

    /// <summary>
    /// Parses a string of the form:
    ///   priceRange[;roomList[;squareRange[;regionList]]]
    /// 
    /// • priceRange  = "min-max"  or "min" (max = null)
    /// • roomList    = "1,2,3"    (comma-separated ints) – optional
    /// • squareRange = "min-max"  or "min" (max = null) – optional
    /// • regionList  = "centre,daugavgriva" – optional
    /// 
    /// Example: "100-200;1,2,3;35-45;centre,daugavgriva"
    /// </summary>
    public static ApartmentFilter FromString(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
            return new ApartmentFilter();

        var filter = new ApartmentFilter();
        var parts = arg.Split(';', StringSplitOptions.RemoveEmptyEntries)
                       .Select(p => p.Trim())
                       .Where(p => !string.IsNullOrEmpty(p))
                       .ToArray();

        // ---- 1. Price -------------------------------------------------

        if (parts.Length > 0)
        {
            ParseRange(parts[0], out var minPrice, out var maxPrice);
            filter.MinPrice = minPrice;
            filter.MaxPrice = maxPrice;
        }
            

        // ---- 2. Rooms -------------------------------------------------
        if (parts.Length > 1)
        {
            filter.Rooms = parts[1]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                .Select(int.Parse)
                .ToList();

            if (!filter.Rooms.Any())
                filter.Rooms = null; // keep null if nothing parsed
        }

        // ---- 3. Square ------------------------------------------------

        if (parts.Length > 2)
        {
            ParseRange(parts[2], out var minSquare, out var maxSquare);
            filter.MinSquare = minSquare;
            filter.MaxSquare = maxSquare;
        }
           

        // ---- 4. Regions -----------------------------------------------
        if (parts.Length > 3)
        {
            filter.Regions = parts[3]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrEmpty(r))
                .ToList();

            if (!filter.Regions.Any())
                filter.Regions = null;
        }

        return filter;
    }

    /// <summary>
    /// Helper that parses "min-max" or "min" into decimal min/max values.
    /// Throws <see cref="FormatException"/> on malformed input.
    /// </summary>
    private static void ParseRange(string segment, out decimal min, out decimal? max)
    {
        min = 0m;
        max = null;

        var rangeParts = segment.Split('-', 2);
        if (rangeParts.Length == 0) return;

        // Min part
        if (!decimal.TryParse(rangeParts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out min))
            throw new FormatException($"Unable to parse minimum value from '{rangeParts[0]}'.");

        // Optional Max part
        if (rangeParts.Length == 2)
        {
            var maxStr = rangeParts[1].Trim();
            if (!string.IsNullOrEmpty(maxStr))
            {
                if (!decimal.TryParse(maxStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxVal))
                    throw new FormatException($"Unable to parse maximum value from '{maxStr}'.");
                max = maxVal;
            }
        }
    }

    // --------------------------------------------------------------------
    // Optional: a nice ToString() for debugging / logging
    public override string ToString()
    {
        return $"Price: {MinPrice}-{MaxPrice ?? (object)"∞"} | " +
               $"Rooms: {(Rooms == null ? "any" : string.Join(",", Rooms))} | " +
               $"Square: {MinSquare}-{MaxSquare ?? (object)"∞"} | " +
               $"Regions: {(Regions == null ? "any" : string.Join(",", Regions))}";
    }
}