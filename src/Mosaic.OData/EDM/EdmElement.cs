namespace Mosaic.OData.EDM;

/// <summary>
/// Base class for all EDM model elements providing common functionality.
/// </summary>
public abstract class EdmElement : IEdmElement
{
    private readonly List<IEdmElement> _children = new();
    private IEdmElement? _parent;

    protected EdmElement(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IEdmElement? Parent => _parent;

    /// <inheritdoc />
    public IReadOnlyList<IEdmElement> Children => _children.AsReadOnly();

    /// <inheritdoc />
    public abstract IEnumerable<(string Name, object? Value)> BasicProperties { get; }

    /// <inheritdoc />
    public abstract string? TargetPathSegment { get; }

    /// <summary>
    /// Adds a child element and sets this element as the child's parent.
    /// Should only be called during model construction.
    /// </summary>
    /// <param name="child">The child element to add.</param>
    internal void AddChild(IEdmElement child)
    {
        _children.Add(child);
        if (child is EdmElement childElement)
        {
            childElement._parent = this;
        }
    }

    /// <summary>
    /// Creates a string representation of this element as an XML element with basic attributes only.
    /// </summary>
    public override string ToString()
    {
        var attributes = string.Join(" ", BasicProperties
            .Where(p => p.Value != null)
            .Select(p => $"{p.Name}=\"{p.Value}\""));
        
        return attributes.Length > 0 
            ? $"<{GetType().Name} {attributes} />"
            : $"<{GetType().Name} />";
    }
}
