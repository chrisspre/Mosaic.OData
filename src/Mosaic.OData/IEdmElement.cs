namespace Mosaic.OData.EDM;

/// <summary>
/// Interface that all EDM model elements must implement to support model navigation and resolution.
/// </summary>
public interface IEdmElement
{
    /// <summary>
    /// Gets the name of this element.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the parent element of this element. Never null except for the root Schema element.
    /// </summary>
    IEdmElement? Parent { get; }

    /// <summary>
    /// Gets the child elements of this element.
    /// </summary>
    IReadOnlyList<IEdmElement> Children { get; }

    /// <summary>
    /// Gets the basic properties of this element as name-value pairs.
    /// </summary>
    IEnumerable<(string Name, object? Value)> BasicProperties { get; }

    /// <summary>
    /// Gets the path segment for this element used in symbolic reference resolution.
    /// Returns null if this element cannot be resolved via path navigation.
    /// The segment includes the appropriate path separator (e.g., ".name" for entities, "/name" for properties).
    /// </summary>
    string? TargetPathSegment { get; }
}
