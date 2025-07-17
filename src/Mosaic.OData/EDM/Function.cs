namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Function element.
/// </summary>
public sealed class Function : EdmElement, IModelElementFactory<Function>
{
    private Path<IEdmElement>? _entitySetPath;

    private Function(string name, bool isBound, bool isComposable, string? entitySetPathExpression) : base(name)
    {
        IsBound = isBound;
        IsComposable = isComposable;
        EntitySetPathExpression = entitySetPathExpression;
    }

    /// <summary>
    /// Gets a value indicating whether this function is bound.
    /// </summary>
    public bool IsBound { get; }

    /// <summary>
    /// Gets a value indicating whether this function is composable.
    /// </summary>
    public bool IsComposable { get; }

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
            if (IsComposable) yield return (nameof(IsComposable), IsComposable);
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
    public static Function Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes.GetRequiredOrDefault("Name", $"<MissingName_{Guid.NewGuid():N}>");
        var isBound = attributes.ParseOrDefault("IsBound", false);
        var isComposable = attributes.ParseOrDefault("IsComposable", true);
        var entitySetPathExpression = attributes.GetValueOrDefault("EntitySetPath");

        var function = new Function(name, isBound, isComposable, entitySetPathExpression);

        // Handle EntitySetPath resolution if present
        if (entitySetPathExpression != null)
        {
            context.AddDeferredAction(400, function, resolutionContext =>
            {
                var entitySetPath = resolutionContext.ResolvePath<IEdmElement>(entitySetPathExpression, function);
                if (entitySetPath != null)
                {
                    function.SetEntitySetPath(entitySetPath);
                }
            }); // Higher priority since it depends on other elements being established
        }

        return function;
    }
}
