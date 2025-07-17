using System.Diagnostics.CodeAnalysis;
using Serilog;

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

    /// <summary>
    /// Gets the required value from the attributes dictionary.
    /// Throws an ArgumentException if the key is not found.
    /// </summary>
    /// <param name="attributes">The attributes dictionary.</param>
    /// <param name="key">The required key.</param>
    /// <returns>The value associated with the key.</returns>
    /// <exception cref="ArgumentException">Thrown when the required key is not found.</exception>
    public static string GetRequired(this IReadOnlyDictionary<string, string> attributes, string key)
    {
        if (attributes.TryGetValue(key, out var value))
        {
            return value;
        }
        throw new ArgumentException($"Required attribute '{key}' not found.", nameof(key));
    }

    /// <summary>
    /// Gets the required value from the attributes dictionary.
    /// Logs a warning and returns a fallback value if the key is not found.
    /// </summary>
    /// <param name="attributes">The attributes dictionary.</param>
    /// <param name="key">The required key.</param>
    /// <param name="fallbackValue">The value to return if the key is not found.</param>
    /// <returns>The value associated with the key, or the fallback value if not found.</returns>
    public static string GetRequiredOrDefault(this IReadOnlyDictionary<string, string> attributes, string key, string fallbackValue = "")
    {
        if (attributes.TryGetValue(key, out var value))
        {
            return value;
        }
                Log.Warning("Required attribute {Key} not found. Using fallback value {FallbackValue}", key, fallbackValue);
        return fallbackValue;
    }

    /// <summary>
    /// Parses a value from the attributes dictionary with a fallback default value.
    /// </summary>
    /// <typeparam name="T">The type to parse the value as, must implement IParsable&lt;T&gt;.</typeparam>
    /// <param name="attributes">The attributes dictionary.</param>
    /// <param name="key">The key to look for in the dictionary.</param>
    /// <param name="defaultValue">The default value to use if parsing fails or key is not found.</param>
    /// <returns>The parsed value or the default value.</returns>
    public static T ParseOrDefault<T>(this IReadOnlyDictionary<string, string> attributes, string key, T defaultValue)
        where T : struct, IParsable<T>
    {
        if (attributes.TryGetValue(key, out var valueStr) && T.TryParse(valueStr, null, out var value))
        {
            return value;
        }
        return defaultValue;
    }
}
