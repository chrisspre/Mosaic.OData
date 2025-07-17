namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM EnumType element.
/// </summary>
public sealed class EnumType : EdmElementBase, IModelElementFactory<EnumType>
{
    private EnumType(string name, string underlyingType, bool isFlags) : base(name)
    {
        UnderlyingType = underlyingType;
        IsFlags = isFlags;
    }

    /// <summary>
    /// Gets the underlying type of this enumeration type.
    /// Must be one of: Edm.Byte, Edm.SByte, Edm.Int16, Edm.Int32, or Edm.Int64.
    /// </summary>
    public string UnderlyingType { get; }

    /// <summary>
    /// Gets a value indicating whether this enumeration type supports flags (multiple values).
    /// </summary>
    public bool IsFlags { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(UnderlyingType), UnderlyingType);
            if (IsFlags) yield return (nameof(IsFlags), IsFlags);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <inheritdoc />
    public static EnumType Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var underlyingType = attributes.GetValueOrDefault("UnderlyingType", "Edm.Int32");
        var isFlags = bool.Parse(attributes.GetValueOrDefault("IsFlags", "false"));

        return new EnumType(name, underlyingType, isFlags);
    }
}
