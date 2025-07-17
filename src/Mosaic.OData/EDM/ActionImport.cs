namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM ActionImport element.
/// </summary>
public sealed class ActionImport : EdmElement, IModelElementFactory<ActionImport>
{
    private ActionImport(string name, string action, string? entitySet) : base(name)
    {
        Action = action;
        EntitySet = entitySet;
    }

    /// <summary>
    /// Gets the qualified unbound action name.
    /// </summary>
    public string Action { get; }

    /// <summary>
    /// Gets the entity set name or target path.
    /// </summary>
    public string? EntitySet { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Action), Action);
            if (EntitySet != null) yield return (nameof(EntitySet), EntitySet);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <inheritdoc />
    public static ActionImport Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes.GetRequiredOrDefault("Name", $"<MissingName_{Guid.NewGuid():N}>");
        var action = attributes.GetRequiredOrDefault("Action", "<MissingAction>");
        var entitySet = attributes.GetValueOrDefault("EntitySet");

        var actionImport = new ActionImport(name, action, entitySet);

        // Handle Action reference resolution (qualified unbound action name)
        context.AddDeferredAction(500, actionImport, resolutionContext =>
        {
            var actionElement = resolutionContext.ResolveReference<Action>(action);
            // Action resolution is for validation purposes - we keep the string reference
        });

        // Handle EntitySet reference resolution (relative to container)
        if (entitySet != null)
        {
            context.AddDeferredAction(500, actionImport, resolutionContext =>
            {
                var entitySetElement = resolutionContext.ResolveRelativeReference<EntitySet>(actionImport, entitySet);
                // EntitySet resolution is for validation purposes - we keep the string reference
            });
        }

        return actionImport;
    }
}
