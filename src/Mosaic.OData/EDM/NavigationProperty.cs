using Serilog;

namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM NavigationProperty element.
/// </summary>
public sealed class NavigationProperty : EdmElement, IModelElementFactory<NavigationProperty>
{
    private Path<NavigationProperty>? _partner;
    private EntityType? _targetType;

    private NavigationProperty(string name, string typeReference, bool nullable, bool containsTarget) : base(name)
    {
        TypeReference = typeReference;
        Nullable = nullable;
        ContainsTarget = containsTarget;
    }

    /// <summary>
    /// Gets the original type reference string from the CSDL.
    /// </summary>
    public string TypeReference { get; }

    /// <summary>
    /// Gets the resolved target entity type.
    /// </summary>
    public EntityType? TargetType => _targetType;

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
            yield return (nameof(TypeReference), TypeReference);
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

    /// <summary>
    /// Sets the target entity type. Should only be called during model resolution.
    /// </summary>
    internal void SetTargetType(EntityType targetType)
    {
        _targetType = targetType;
    }

    /// <inheritdoc />
    public static NavigationProperty Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes.GetRequiredOrDefault("Name", $"<MissingName_{Guid.NewGuid():N}>");
        var type = attributes.GetRequiredOrDefault("Type", "<MissingType>");
        var nullable = attributes.ParseOrDefault("Nullable", true);
        var containsTarget = attributes.ParseOrDefault("ContainsTarget", false);

        var navigationProperty = new NavigationProperty(name, type, nullable, containsTarget);

        // Handle Type reference resolution (resolve to EntityType)
        context.AddDeferredAction(100, navigationProperty, resolutionContext =>
        {
            var targetType = resolutionContext.ResolveReference<EntityType>(type);
            if (targetType != null)
            {
                navigationProperty.SetTargetType(targetType);
            }
        }); // Lower priority to ensure entity types are created first

        // Handle Partner path resolution
        if (attributes.TryGetValue("Partner", out var partnerName))
        {
            context.AddDeferredAction(200, navigationProperty, resolutionContext =>
            {
                // Now we can use the resolved target type to find the partner
                var targetType = navigationProperty.TargetType;
                if (targetType != null)
                {
                    // Look for the partner NavigationProperty in the target EntityType
                    var partner = targetType.Children.OfType<NavigationProperty>()
                        .FirstOrDefault(np => np.Name == partnerName);
                    
                    if (partner != null)
                    {
                        navigationProperty.SetPartner(new Path<NavigationProperty>(new IEdmElement[] { partner }));
                    }
                    else
                    {
                        Log.Warning("Unable to find partner NavigationProperty {PartnerName} on EntityType {TargetTypeName}", partnerName, targetType.Name);
                    }
                }
                // If target type resolution fails, the NavigationProperty will remain without a partner
            }); // Higher priority since it depends on the target type being resolved
        }

        return navigationProperty;
    }
}
