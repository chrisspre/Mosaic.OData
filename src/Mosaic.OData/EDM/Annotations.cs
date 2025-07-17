namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Annotations element.
/// </summary>
public sealed class Annotations : EdmElementBase, IModelElementFactory<Annotations>
{
    private Path<IEdmElement>? _target;

    private Annotations(string targetExpression, string? qualifier) : base("Annotations")
    {
        TargetExpression = targetExpression;
        Qualifier = qualifier;
    }

    /// <summary>
    /// Gets the target path expression as a string.
    /// </summary>
    public string TargetExpression { get; }

    /// <summary>
    /// Gets the qualifier for these annotations.
    /// </summary>
    public string? Qualifier { get; }

    /// <summary>
    /// Gets the resolved target path.
    /// </summary>
    public Path<IEdmElement>? Target => _target;

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(TargetExpression), TargetExpression);
            if (Qualifier != null) yield return (nameof(Qualifier), Qualifier);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // Annotations is not resolvable via path navigation

    /// <summary>
    /// Sets the target path. Should only be called during model resolution.
    /// </summary>
    internal void SetTarget(Path<IEdmElement> target)
    {
        _target = target;
    }

    /// <inheritdoc />
    public static Annotations Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var targetExpression = attributes["Target"];
        var qualifier = attributes.GetValueOrDefault("Qualifier");

        var annotations = new Annotations(targetExpression, qualifier);

        // Handle Target path resolution
        context.AddDeferredAction(new DeferredAction(annotations, resolutionContext =>
        {
            var target = resolutionContext.ResolvePath<IEdmElement>(targetExpression, annotations);
            if (target != null)
            {
                annotations.SetTarget(target);
            }
        }), priority: 600); // Higher priority since it depends on all other elements being established

        return annotations;
    }
}
