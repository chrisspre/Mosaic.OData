namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM ReferentialConstraint element.
/// </summary>
public sealed class ReferentialConstraint : EdmElementBase, IModelElementFactory<ReferentialConstraint>
{
    private Path<Property>? _property;
    private Path<Property>? _referencedProperty;

    private ReferentialConstraint(string propertyPath, string referencedPropertyPath) : base("ReferentialConstraint")
    {
        PropertyPath = propertyPath;
        ReferencedPropertyPath = referencedPropertyPath;
    }

    /// <summary>
    /// Gets the path to the dependent property.
    /// </summary>
    public string PropertyPath { get; }

    /// <summary>
    /// Gets the path to the principal property.
    /// </summary>
    public string ReferencedPropertyPath { get; }

    /// <summary>
    /// Gets the resolved dependent property.
    /// </summary>
    public Path<Property>? Property => _property;

    /// <summary>
    /// Gets the resolved principal property.
    /// </summary>
    public Path<Property>? ReferencedProperty => _referencedProperty;

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(PropertyPath), PropertyPath);
            yield return (nameof(ReferencedPropertyPath), ReferencedPropertyPath);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // ReferentialConstraint is not resolvable via path navigation

    /// <summary>
    /// Sets the property paths. Should only be called during model resolution.
    /// </summary>
    internal void SetProperties(Path<Property> property, Path<Property> referencedProperty)
    {
        _property = property;
        _referencedProperty = referencedProperty;
    }

    /// <inheritdoc />
    public static ReferentialConstraint Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var propertyPath = attributes["Property"];
        var referencedPropertyPath = attributes["ReferencedProperty"];

        var constraint = new ReferentialConstraint(propertyPath, referencedPropertyPath);

        // Resolve both property paths
        context.AddDeferredAction(new DeferredAction(constraint, resolutionContext =>
        {
            var property = resolutionContext.ResolvePath<Property>(propertyPath, constraint);
            var referencedProperty = resolutionContext.ResolvePath<Property>(referencedPropertyPath, constraint);
            
            if (property != null && referencedProperty != null)
            {
                constraint.SetProperties(property, referencedProperty);
            }
            // If either property resolution fails, the constraint will keep the original string paths
        }), priority: 300); // Higher priority since it depends on properties being established

        return constraint;
    }
}
