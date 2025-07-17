namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM ComplexType element.
/// </summary>
public sealed class ComplexType : EdmElementBase, IModelElementFactory<ComplexType>
{
    private ComplexType? _baseType;

    private ComplexType(string name, bool isAbstract, bool openType) : base(name)
    {
        Abstract = isAbstract;
        OpenType = openType;
    }

    /// <summary>
    /// Gets the base type of this complex type.
    /// </summary>
    public ComplexType? BaseType => _baseType;

    /// <summary>
    /// Gets a value indicating whether this complex type is abstract.
    /// </summary>
    public bool Abstract { get; }

    /// <summary>
    /// Gets a value indicating whether this complex type is open.
    /// </summary>
    public bool OpenType { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            if (Abstract) yield return (nameof(Abstract), Abstract);
            if (OpenType) yield return (nameof(OpenType), OpenType);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <summary>
    /// Sets the base type. Should only be called during model resolution.
    /// </summary>
    internal void SetBaseType(ComplexType baseType)
    {
        _baseType = baseType;
    }

    /// <inheritdoc />
    public static ComplexType Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var isAbstract = bool.Parse(attributes.GetValueOrDefault("Abstract", "false"));
        var openType = bool.Parse(attributes.GetValueOrDefault("OpenType", "false"));

        var complexType = new ComplexType(name, isAbstract, openType);

        // Handle BaseType reference resolution
        if (attributes.TryGetValue("BaseType", out var baseTypeRef))
        {
            context.AddDeferredAction(new DeferredAction(complexType, resolutionContext =>
            {
                var baseType = resolutionContext.ResolveReference<ComplexType>(baseTypeRef);
                if (baseType != null)
                {
                    complexType.SetBaseType(baseType);
                }
            }), priority: 100); // Lower priority to ensure types are created first
        }

        return complexType;
    }
}
