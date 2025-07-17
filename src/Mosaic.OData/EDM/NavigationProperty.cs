namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM NavigationProperty element.
/// </summary>
public sealed class NavigationProperty : EdmElementBase, IModelElementFactory<NavigationProperty>
{
    private Path<NavigationProperty>? _partner;

    private NavigationProperty(string name, string type, bool nullable, bool containsTarget) : base(name)
    {
        Type = type;
        Nullable = nullable;
        ContainsTarget = containsTarget;
    }

    /// <summary>
    /// Gets the type of this navigation property.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets a value indicating whether this navigation property can be null.
    /// </summary>
    public bool Nullable { get; }

    /// <summary>
    /// Gets the partner navigation property.
    /// </summary>
    public Path<NavigationProperty>? Partner => _partner;

    /// <summary>
    /// Gets a value indicating whether this navigation property contains the target.
    /// </summary>
    public bool ContainsTarget { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Type), Type);
            if (!Nullable) yield return (nameof(Nullable), Nullable);
            if (ContainsTarget) yield return (nameof(ContainsTarget), ContainsTarget);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $"/{Name}";

    /// <summary>
    /// Sets the partner navigation property. Should only be called during model resolution.
    /// </summary>
    internal void SetPartner(Path<NavigationProperty> partner)
    {
        _partner = partner;
    }

    /// <inheritdoc />
    public static NavigationProperty Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var type = attributes["Type"];
        var nullable = bool.Parse(attributes.GetValueOrDefault("Nullable", "true"));
        var containsTarget = bool.Parse(attributes.GetValueOrDefault("ContainsTarget", "false"));

        var navigationProperty = new NavigationProperty(name, type, nullable, containsTarget);

        // Handle Partner path resolution
        if (attributes.TryGetValue("Partner", out var partnerName))
        {
            context.AddDeferredAction(new DeferredAction(navigationProperty, resolutionContext =>
            {
                // Find the partner NavigationProperty in the target type
                var partner = resolutionContext.FindElementInType<NavigationProperty>(type, partnerName);
                
                if (partner != null)
                {
                    navigationProperty.SetPartner(new Path<NavigationProperty>(new IEdmElement[] { partner }));
                }
                // If partner resolution fails, the NavigationProperty will remain without a partner
            }), priority: 200); // Higher priority since it depends on paths being established
        }

        return navigationProperty;
    }
}
