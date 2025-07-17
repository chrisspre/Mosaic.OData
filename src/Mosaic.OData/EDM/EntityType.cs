namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM EntityType element.
/// </summary>
public sealed class EntityType : EdmElementBase, IModelElementFactory<EntityType>
{
    private EntityType? _baseType;

    private EntityType(string name, bool isAbstract, bool openType, bool hasStream) : base(name)
    {
        Abstract = isAbstract;
        OpenType = openType;
        HasStream = hasStream;
    }

    /// <summary>
    /// Gets the base type of this entity type.
    /// </summary>
    public EntityType? BaseType => _baseType;

    /// <summary>
    /// Gets a value indicating whether this entity type is abstract.
    /// </summary>
    public bool Abstract { get; }

    /// <summary>
    /// Gets a value indicating whether this entity type is open.
    /// </summary>
    public bool OpenType { get; }

    /// <summary>
    /// Gets a value indicating whether this entity type has stream.
    /// </summary>
    public bool HasStream { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            if (Abstract) yield return (nameof(Abstract), Abstract);
            if (OpenType) yield return (nameof(OpenType), OpenType);
            if (HasStream) yield return (nameof(HasStream), HasStream);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <summary>
    /// Sets the base type. Should only be called during model resolution.
    /// </summary>
    internal void SetBaseType(EntityType baseType)
    {
        _baseType = baseType;
    }

    /// <inheritdoc />
    public static EntityType Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes.GetValueOrDefault("Name", "<missing Name>");
        var isAbstract = bool.Parse(attributes.GetValueOrDefault("Abstract", "false"));
        var openType = bool.Parse(attributes.GetValueOrDefault("OpenType", "false"));
        var hasStream = bool.Parse(attributes.GetValueOrDefault("HasStream", "false"));

        var entityType = new EntityType(name, isAbstract, openType, hasStream);

        // Handle BaseType reference resolution
        if (attributes.TryGetValue("BaseType", out var baseTypeRef))
        {
            context.AddDeferredAction(new DeferredAction(entityType, resolutionContext =>
            {
                var baseType = resolutionContext.ResolveReference<EntityType>(baseTypeRef);
                if (baseType != null)
                {
                    entityType.SetBaseType(baseType);
                }
            }), priority: 100); // Lower priority to ensure types are created first
        }

        return entityType;
    }
}
