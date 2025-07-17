namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM EntityContainer element.
/// </summary>
public sealed class EntityContainer : EdmElementBase, IModelElementFactory<EntityContainer>
{
    private EntityContainer? _extends;

    private EntityContainer(string name, string? extendsReference) : base(name)
    {
        ExtendsReference = extendsReference;
    }

    /// <summary>
    /// Gets the name of the entity container that this container extends.
    /// </summary>
    public string? ExtendsReference { get; }

    /// <summary>
    /// Gets the entity container that this container extends.
    /// </summary>
    public EntityContainer? Extends => _extends;

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            if (ExtendsReference != null) yield return (nameof(ExtendsReference), ExtendsReference);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <summary>
    /// Sets the extended container. Should only be called during model resolution.
    /// </summary>
    internal void SetExtends(EntityContainer extends)
    {
        _extends = extends;
    }

    /// <inheritdoc />
    public static EntityContainer Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var extendsReference = attributes.GetValueOrDefault("Extends");

        var container = new EntityContainer(name, extendsReference);

        // Handle Extends reference resolution
        if (extendsReference != null)
        {
            context.AddDeferredAction(new DeferredAction(container, resolutionContext =>
            {
                var extendsContainer = resolutionContext.ResolveReference<EntityContainer>(extendsReference);
                if (extendsContainer != null)
                {
                    container.SetExtends(extendsContainer);
                }
            }), priority: 100); // Lower priority to ensure containers are created first
        }

        return container;
    }
}
