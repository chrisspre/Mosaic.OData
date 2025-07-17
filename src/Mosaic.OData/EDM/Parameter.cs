namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Parameter element.
/// </summary>
public sealed class Parameter : EdmElement, IModelElementFactory<Parameter>
{
    private Parameter(string name, string type, bool nullable) : base(name)
    {
        Type = type;
        Nullable = nullable;
    }

    /// <summary>
    /// Gets the type of this parameter.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets a value indicating whether this parameter can be null.
    /// </summary>
    public bool Nullable { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Type), Type);
            if (!Nullable) yield return (nameof(Nullable), Nullable);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // Parameters are not resolvable via path navigation

    /// <inheritdoc />
    public static Parameter Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes.GetRequiredOrDefault("Name", $"<MissingName_{Guid.NewGuid():N}>");
        var type = attributes.GetRequiredOrDefault("Type", "<MissingType>");
        var nullable = attributes.ParseOrDefault("Nullable", true);

        return new Parameter(name, type, nullable);
    }
}
