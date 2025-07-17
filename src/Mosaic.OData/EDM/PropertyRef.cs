namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM PropertyRef element within a Key.
/// </summary>
public sealed class PropertyRef : EdmElement, IModelElementFactory<PropertyRef>
{
    private Path<Property>? _property;

    private PropertyRef(string name, string? alias) : base(name)
    {
        Alias = alias;
    }

    /// <summary>
    /// Gets the alias for this property reference.
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// Gets the path to the structural property that this property reference points to.
    /// </summary>
    public Path<Property>? Property => _property;

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            if (Alias != null) yield return (nameof(Alias), Alias);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // PropertyRef is not resolvable via path navigation

    /// <summary>
    /// Sets the property path. Should only be called during model resolution.
    /// </summary>
    internal void SetProperty(Path<Property> property)
    {
        _property = property;
    }

    /// <inheritdoc />
    public static PropertyRef Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var alias = attributes.GetValueOrDefault("Alias");

        var propertyRef = new PropertyRef(name, alias);

        // Resolve the property path - the Name attribute is actually a path to the property
        context.AddDeferredAction(300, propertyRef, resolutionContext =>
        {
            var property = resolutionContext.ResolvePath<Property>(name, propertyRef);
            if (property != null)
            {
                propertyRef.SetProperty(property);
            }
            // If property resolution fails, the PropertyRef will keep the original name string
        }); // Higher priority since it depends on properties being established

        return propertyRef;
    }
}
