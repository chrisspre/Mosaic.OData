namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Property element.
/// </summary>
public sealed class Property : EdmElementBase, IModelElementFactory<Property>
{
    private Property(string name, string type, bool nullable, string? defaultValue, int? maxLength, int? precision, Scale? scale, bool? unicode, string? srid) : base(name)
    {
        Type = type;
        Nullable = nullable;
        DefaultValue = defaultValue;
        MaxLength = maxLength;
        Precision = precision;
        Scale = scale;
        Unicode = unicode;
        SRID = srid;
    }

    /// <summary>
    /// Gets the type of this property.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets a value indicating whether this property can be null.
    /// </summary>
    public bool Nullable { get; }

    /// <summary>
    /// Gets the default value of this property.
    /// </summary>
    public string? DefaultValue { get; }

    /// <summary>
    /// Gets the maximum length for string properties.
    /// </summary>
    public int? MaxLength { get; }

    /// <summary>
    /// Gets the precision for decimal/temporal properties.
    /// </summary>
    public int? Precision { get; }

    /// <summary>
    /// Gets the scale for decimal properties.
    /// </summary>
    public Scale? Scale { get; }

    /// <summary>
    /// Gets a value indicating whether string properties support Unicode.
    /// </summary>
    public bool? Unicode { get; }

    /// <summary>
    /// Gets the spatial reference system identifier for geometry/geography properties.
    /// </summary>
    public string? SRID { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Type), Type);
            if (!Nullable) yield return (nameof(Nullable), Nullable);
            if (DefaultValue != null) yield return (nameof(DefaultValue), DefaultValue);
            if (MaxLength.HasValue) yield return (nameof(MaxLength), MaxLength.Value);
            if (Precision.HasValue) yield return (nameof(Precision), Precision.Value);
            if (Scale.HasValue) yield return (nameof(Scale), Scale.Value);
            if (Unicode.HasValue) yield return (nameof(Unicode), Unicode.Value);
            if (SRID != null) yield return (nameof(SRID), SRID);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $"/{Name}";

    /// <inheritdoc />
    public static Property Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var type = attributes["Type"];
        var nullable = bool.Parse(attributes.GetValueOrDefault("Nullable", "true"));
        var defaultValue = attributes.GetValueOrDefault("DefaultValue");

        var maxLength = attributes.TryGetValue("MaxLength", out var maxLengthStr) && int.TryParse(maxLengthStr, out var parsedMaxLength)
            ? (int?)parsedMaxLength
            : null;

        var precision = attributes.TryGetValue("Precision", out var precisionStr) && int.TryParse(precisionStr, out var parsedPrecision)
            ? (int?)parsedPrecision
            : null;

        // var scale = attributes.TryGetValue("Scale", out var scaleStr) && int.TryParse(scaleStr, out var parsedScale)
        //     ? (int?)parsedScale
        //     : null;
        var scale = attributes.ParseOrDefault<Scale>("Scale");
        var unicode = attributes.ParseOrDefault<bool>("Unicode");

        var srid = attributes.GetValueOrDefault("SRID");

        return new Property(name, type, nullable, defaultValue, maxLength, precision, scale, unicode, srid);
    }
}
