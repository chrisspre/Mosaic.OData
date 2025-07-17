namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM NavigationPropertyBinding element.
/// </summary>
public sealed class NavigationPropertyBinding : EdmElement, IModelElementFactory<NavigationPropertyBinding>
{
    private Path<NavigationProperty>? _path;
    private Path<IEdmElement>? _target;

    private NavigationPropertyBinding(string pathExpression, string targetExpression) : base("NavigationPropertyBinding")
    {
        PathExpression = pathExpression;
        TargetExpression = targetExpression;
    }

    /// <summary>
    /// Gets the navigation property path expression as a string.
    /// </summary>
    public string PathExpression { get; }

    /// <summary>
    /// Gets the target path expression as a string.
    /// </summary>
    public string TargetExpression { get; }

    /// <summary>
    /// Gets the resolved navigation property path.
    /// </summary>
    public Path<NavigationProperty>? Path => _path;

    /// <summary>
    /// Gets the resolved target path.
    /// </summary>
    public Path<IEdmElement>? Target => _target;

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(PathExpression), PathExpression);
            yield return (nameof(TargetExpression), TargetExpression);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // NavigationPropertyBinding is not resolvable via path navigation

    /// <summary>
    /// Sets the resolved paths. Should only be called during model resolution.
    /// </summary>
    internal void SetPaths(Path<NavigationProperty> path, Path<IEdmElement> target)
    {
        _path = path;
        _target = target;
    }

    /// <inheritdoc />
    public static NavigationPropertyBinding Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        if (!attributes.TryGetValue("Path", out var pathExpression))
        {
            throw new EdmElementCreationException("NavigationPropertyBinding", $"Missing required 'Path' attribute. Available attributes: {string.Join(", ", attributes.Keys)}");
        }
        
        if (!attributes.TryGetValue("Target", out var targetExpression))
        {
            throw new EdmElementCreationException("NavigationPropertyBinding", $"Missing required 'Target' attribute. Available attributes: {string.Join(", ", attributes.Keys)}");
        }

        var binding = new NavigationPropertyBinding(pathExpression, targetExpression);

        // Handle path resolution
        context.AddDeferredAction(300, binding, resolutionContext =>
        {
            // Path should resolve relative to the EntitySet's EntityType
            // First, find the parent EntitySet
            var entitySet = binding.Parent as EntitySet;
            if (entitySet != null)
            {
                // Resolve the EntityType of this EntitySet
                var entityType = resolutionContext.ResolveReference<EntityType>(entitySet.EntityType);
                if (entityType != null)
                {
                    // Find the NavigationProperty within the EntityType
                    var navigationProperty = entityType.Children.OfType<NavigationProperty>()
                        .FirstOrDefault(np => np.Name == pathExpression);
                    
                    // Target should resolve relative to the EntityContainer
                    var targetElement = resolutionContext.ResolveRelativeReference<IEdmElement>(binding, targetExpression);
                    
                    if (navigationProperty != null && targetElement != null)
                    {
                        var path = new Path<NavigationProperty>(new IEdmElement[] { navigationProperty });
                        var target = new Path<IEdmElement>(new IEdmElement[] { targetElement });
                        binding.SetPaths(path, target);
                    }
                }
            }
            // If resolution fails, the binding will remain with the original string expressions
        }); // Higher priority since it depends on navigation properties being established

        return binding;
    }
}
