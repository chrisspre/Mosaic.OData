namespace Mosaic.OData.EDM;

/// <summary>
/// Represents a path to an element in the EDM model, consisting of a sequence of elements.
/// </summary>
/// <typeparam name="T">The type of the target element that the path resolves to.</typeparam>
public class Path<T> where T : IEdmElement
{
    private readonly IReadOnlyList<IEdmElement> _elements;

    public Path(IEnumerable<IEdmElement> elements)
    {
        _elements = elements?.ToList() ?? throw new ArgumentNullException(nameof(elements));
        if (_elements.Count == 0)
            throw new ArgumentException("Path must contain at least one element.", nameof(elements));
        
        if (_elements.Last() is not T)
            throw new ArgumentException($"The last element in the path must be of type {typeof(T).Name}.", nameof(elements));
    }

    /// <summary>
    /// Gets the sequence of elements that make up this path.
    /// </summary>
    public IReadOnlyList<IEdmElement> Elements => _elements;

    /// <summary>
    /// Gets the target element at the end of the path.
    /// </summary>
    public T Target => (T)_elements.Last();

    /// <summary>
    /// Creates a string representation of the path using element path segments.
    /// </summary>
    public override string ToString()
    {
        return string.Join("", _elements.Select(e => e.TargetPathSegment ?? ""));
    }
}
