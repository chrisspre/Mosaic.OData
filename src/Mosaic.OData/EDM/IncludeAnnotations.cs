namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDMX IncludeAnnotations element.
/// </summary>
public sealed class IncludeAnnotations : EdmElement, IModelElementFactory<IncludeAnnotations>
{
    private IncludeAnnotations(string termNamespace, string? qualifier, string? targetNamespace) : base("IncludeAnnotations")
    {
        TermNamespace = termNamespace;
        Qualifier = qualifier;
        TargetNamespace = targetNamespace;
    }

    /// <summary>
    /// Gets the namespace of the terms to include annotations for.
    /// </summary>
    public string TermNamespace { get; }

    /// <summary>
    /// Gets the qualifier for the annotations to include.
    /// </summary>
    public string? Qualifier { get; }

    /// <summary>
    /// Gets the target namespace to include annotations for.
    /// </summary>
    public string? TargetNamespace { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(TermNamespace), TermNamespace);
            if (Qualifier != null) yield return (nameof(Qualifier), Qualifier);
            if (TargetNamespace != null) yield return (nameof(TargetNamespace), TargetNamespace);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // IncludeAnnotations has no path segment

    /// <inheritdoc />
    public static IncludeAnnotations Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var termNamespace = attributes.GetRequiredOrDefault("TermNamespace", "UnknownTermNamespace");
        var qualifier = attributes.GetValueOrDefault("Qualifier");
        var targetNamespace = attributes.GetValueOrDefault("TargetNamespace");
        return new IncludeAnnotations(termNamespace, qualifier, targetNamespace);
    }
}
