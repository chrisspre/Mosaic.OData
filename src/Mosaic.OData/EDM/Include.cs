namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDMX Include element.
/// </summary>
public sealed class Include : EdmElementBase, IModelElementFactory<Include>
{
    private Include(string @namespace, string? alias) : base("Include")
    {
        Namespace = @namespace;
        Alias = alias;
    }

    /// <summary>
    /// Gets the namespace to include.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Gets the alias for the included namespace.
    /// </summary>
    public string? Alias { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Namespace), Namespace);
            if (Alias != null) yield return (nameof(Alias), Alias);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // Include has no path segment

    /// <inheritdoc />
    public static Include Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var @namespace = attributes["Namespace"];
        var alias = attributes.GetValueOrDefault("Alias");
        return new Include(@namespace, alias);
    }
}
