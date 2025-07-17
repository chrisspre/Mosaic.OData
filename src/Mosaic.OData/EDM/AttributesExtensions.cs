using System.Diagnostics.CodeAnalysis;

namespace Mosaic.OData.EDM;

public static class AttributesExtensions
{

    /// <summary>
    /// Parses a value from the dictionary as a Nullable<typeparamref name="T"/>>.
    /// Returns null if the key is not found or if parsing fails.
    /// </summary>
    /// <typeparam name="T">The type to parse the value as, must implement IParsable&lt;T&gt;.</typeparam>
    /// <param name="attributes">The dictionary containing the attributes.</param>
    /// <param name="Name">The key to look for in the dictionary.</param>
    /// <returns>
    /// A nullable value of type <typeparamref name="T"/> if found and parsed successfully
    /// or null if the key is not found or parsing fails.
    /// </returns>
    public static T? ParseOrDefault<T>(this IReadOnlyDictionary<string, string> attributes, string Name)
        where T : struct, IParsable<T>
    {
        if(attributes.TryGetValue(Name, out var valueStr) && T.TryParse(valueStr, null, out var value))
        {
            return value;
        }
        return default;
    }

    public static bool TryParse<T>(this IReadOnlyDictionary<string, string> attributes, string Name, [MaybeNullWhen(false)] out T value)
        where T : IParsable<T>
    {
        if (attributes.TryGetValue(Name, out var valueStr) && T.TryParse(valueStr, null, out var actualValue))
        {
            value = actualValue;
            return true;
        }
        value = default;
        return false;
    }
}