namespace SS.Parser;

public static class ListExtensions
{
    /// <summary>
    /// Computes a combined hash code for the entire list based on its items.
    /// Assumes T has proper GetHashCode() and Equals() overrides.
    /// Order of items matters—if order is irrelevant, sort the list first.
    /// </summary>
    /// <typeparam name="T">The type of items in the list (e.g., ApartmentModel).</typeparam>
    /// <param name="list">The list to hash.</param>
    /// <returns>A 32-bit hash code for the list.</returns>
    public static int GetCombinedHashCode<T>(this IEnumerable<T> list) where T : notnull
    {
        if (list == null) throw new ArgumentNullException(nameof(list));

        var hash = new HashCode();
        foreach (var item in list)
        {
            hash.Add(item); // Relies on item's GetHashCode()
        }
        return hash.ToHashCode();

        // Alternative manual implementation (for older .NET without HashCode struct):
        // unchecked
        // {
        //     int hashCode = 17;
        //     foreach (var item in list)
        //     {
        //         hashCode = hashCode * 23 + (item?.GetHashCode() ?? 0);
        //     }
        //     return hashCode;
        // }
    }

    /// <summary>
    /// Checks if two lists are equal (same items in same order, using item.Equals()).
    /// Use this alongside hash codes for reliable comparison.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="list1">First list.</param>
    /// <param name="list2">Second list.</param>
    /// <returns>True if lists match exactly.</returns>
    public static bool ListsEqual<T>(this IEnumerable<T>? list1, IEnumerable<T>? list2)
    {
        if (list1 == null && list2 == null) return true;
        if (list1 == null || list2 == null) return false;
        return list1.SequenceEqual(list2); // Uses IEquatable<T> or Equals() on items
    }
}