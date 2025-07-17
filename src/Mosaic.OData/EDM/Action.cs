namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Action element.
/// </summary>
public sealed class Action : EdmElement, IModelElementFactory<Action>
{
    private Path<IEdmElement>? _entitySetPath;

    private Action(string name, bool isBound, string? entitySetPathExpression) : base(name)
    {
        IsBound = isBound;
        EntitySetPathExpression = entitySetPathExpression;
    }

    /// <summary>
    /// Gets a value indicating whether this action is bound.
    /// </summary>
    public bool IsBound { get; }

    /// <summary>
    /// Gets the entity set path expression as a string.
    /// </summary>
    public string? EntitySetPathExpression { get; }

    /// <summary>
    /// Gets the resolved entity set path.
    /// </summary>
    public Path<IEdmElement>? EntitySetPath => _entitySetPath;

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            if (IsBound) yield return (nameof(IsBound), IsBound);
            if (EntitySetPathExpression != null) yield return (nameof(EntitySetPathExpression), EntitySetPathExpression);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <summary>
    /// Sets the entity set path. Should only be called during model resolution.
    /// </summary>
    internal void SetEntitySetPath(Path<IEdmElement> entitySetPath)
    {
        _entitySetPath = entitySetPath;
    }

    /// <inheritdoc />
    public static Action Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var isBound = bool.Parse(attributes.GetValueOrDefault("IsBound", "false"));
        var entitySetPathExpression = attributes.GetValueOrDefault("EntitySetPath");

        var action = new Action(name, isBound, entitySetPathExpression);

        // Handle EntitySetPath resolution if present
        if (entitySetPathExpression != null)
        {
            context.AddDeferredAction(400, action, resolutionContext =>
            {
                var entitySetPath = resolutionContext.ResolvePath<IEdmElement>(entitySetPathExpression, action);
                if (entitySetPath != null)
                {
                    action.SetEntitySetPath(entitySetPath);
                }
            }); // Higher priority since it depends on other elements being established
        }

        return action;
    }
}
