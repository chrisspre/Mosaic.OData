namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM EntitySet element.
/// </summary>
public sealed class EntitySet : EdmElement, IModelElementFactory<EntitySet>
{
    private EntitySet(string name, string entityType, bool includeInServiceDocument) : base(name)
    {
        EntityType = entityType;
        IncludeInServiceDocument = includeInServiceDocument;
    }

    /// <summary>
    /// Gets the qualified entity type name for this entity set.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets a value indicating whether this entity set should be included in the service document.
    /// </summary>
    public bool IncludeInServiceDocument { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(EntityType), EntityType);
            if (!IncludeInServiceDocument) yield return (nameof(IncludeInServiceDocument), IncludeInServiceDocument);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <inheritdoc />
    public static EntitySet Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var entityType = attributes["EntityType"];
        var includeInServiceDocument = bool.Parse(attributes.GetValueOrDefault("IncludeInServiceDocument", "true"));

        return new EntitySet(name, entityType, includeInServiceDocument);
    }
}
