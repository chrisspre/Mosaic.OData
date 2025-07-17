namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM TypeDefinition element.
/// </summary>
public sealed class TypeDefinition : EdmElement, IModelElementFactory<TypeDefinition>
{
    private TypeDefinition(string name, string underlyingType) : base(name)
    {
        UnderlyingType = underlyingType;
    }

    /// <summary>
    /// Gets the underlying primitive type of this type definition.
    /// Cannot be another type definition.
    /// </summary>
    public string UnderlyingType { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(UnderlyingType), UnderlyingType);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <inheritdoc />
    public static TypeDefinition Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes.GetRequiredOrDefault("Name", $"<MissingName_{Guid.NewGuid():N}>");
        var underlyingType = attributes.GetRequiredOrDefault("UnderlyingType", "<MissingUnderlyingType>");

        return new TypeDefinition(name, underlyingType);
    }
}
