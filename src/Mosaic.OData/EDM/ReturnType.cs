namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM ReturnType element.
/// </summary>
public sealed class ReturnType : EdmElementBase, IModelElementFactory<ReturnType>
{
    private ReturnType(string type, bool nullable) : base("ReturnType")
    {
        Type = type;
        Nullable = nullable;
    }

    /// <summary>
    /// Gets the type of the return value.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets a value indicating whether the return value can be null.
    /// </summary>
    public bool Nullable { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Type), Type);
            if (!Nullable) yield return (nameof(Nullable), Nullable);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // ReturnType is not resolvable via path navigation

    /// <inheritdoc />
    public static ReturnType Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var type = attributes["Type"];
        var nullable = bool.Parse(attributes.GetValueOrDefault("Nullable", "true"));

        return new ReturnType(type, nullable);
    }
}
