namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Singleton element.
/// </summary>
public sealed class Singleton : EdmElementBase, IModelElementFactory<Singleton>
{
    private Singleton(string name, string type, bool nullable) : base(name)
    {
        Type = type;
        Nullable = nullable;
    }

    /// <summary>
    /// Gets the qualified entity type name for this singleton.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets a value indicating whether this singleton can be null.
    /// </summary>
    public bool Nullable { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Type), Type);
            if (Nullable) yield return (nameof(Nullable), Nullable);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <inheritdoc />
    public static Singleton Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var type = attributes["Type"];
        var nullable = bool.Parse(attributes.GetValueOrDefault("Nullable", "false"));

        return new Singleton(name, type, nullable);
    }
}
