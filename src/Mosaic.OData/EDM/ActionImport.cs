namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM ActionImport element.
/// </summary>
public sealed class ActionImport : EdmElementBase, IModelElementFactory<ActionImport>
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
        var name = attributes["Name"];
        var action = attributes["Action"];
        var entitySet = attributes.GetValueOrDefault("EntitySet");

        var actionImport = new ActionImport(name, action, entitySet);

        // Handle Action reference resolution (qualified unbound action name)
        context.AddDeferredAction(new DeferredAction(actionImport, resolutionContext =>
        {
            var actionElement = resolutionContext.ResolveReference<Action>(action);
            // Action resolution is for validation purposes - we keep the string reference
        }), priority: 500);

        // Handle EntitySet reference resolution (relative to container)
        if (entitySet != null)
        {
            context.AddDeferredAction(new DeferredAction(actionImport, resolutionContext =>
            {
                var entitySetElement = resolutionContext.ResolveRelativeReference<EntitySet>(actionImport, entitySet);
                // EntitySet resolution is for validation purposes - we keep the string reference
            }), priority: 500);
        }

        return actionImport;
    }
}
