using System.Reflection;

namespace SS.Notifier.Data.Extensions;

public static class EntityCopyExtensions
{
    /// <summary>
    /// Copies all public instance properties from <paramref name="source"/>
    /// to <paramref name="target"/> **except** the primary-key property (Id).
    /// </summary>
    public static void CopyTo<T>(this T source, T target, params string[] exclude)
        where T : class
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (target == null) throw new ArgumentNullException(nameof(target));

        var excludeSet = new HashSet<string>(exclude, StringComparer.OrdinalIgnoreCase);
        excludeSet.Add("Id");                     // never overwrite the PK

        var props = typeof(T).GetProperties(
                BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite && !excludeSet.Contains(p.Name));

        foreach (var prop in props)
        {
            var value = prop.GetValue(source);
            prop.SetValue(target, value);
        }
    }
}