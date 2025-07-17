namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Schema element.
/// </summary>
public sealed class Schema : EdmElement, IModelElementFactory<Schema>
{
    private Schema(string name, string? @namespace, string? alias) : base(name)
    {
        Namespace = @namespace;
        Alias = alias;
    }

    /// <summary>
    /// Gets the namespace of the schema.
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Gets the alias of the schema.
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
    public override string? TargetPathSegment => null; // Schema is the root, no path segment

    /// <inheritdoc />
    public static Schema Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes.TryGetValue("Namespace", out var ns) ? ns : "DefaultSchema";
        var @namespace = attributes.GetValueOrDefault("Namespace");
        var alias = attributes.GetValueOrDefault("Alias");

        return new Schema(name, @namespace, alias);
    }
}
