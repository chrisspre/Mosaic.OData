namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Annotation element.
/// </summary>
public sealed class Annotation : EdmElementBase, IModelElementFactory<Annotation>
{
    private Annotation(string term, string? qualifier) : base("Annotation")
    {
        Term = term;
        Qualifier = qualifier;
    }

    /// <summary>
    /// Gets the qualified term name.
    /// </summary>
    public string Term { get; }

    /// <summary>
    /// Gets the qualifier for this annotation.
    /// </summary>
    public string? Qualifier { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Term), Term);
            if (Qualifier != null) yield return (nameof(Qualifier), Qualifier);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // Annotation is not resolvable via path navigation

    /// <inheritdoc />
    public static Annotation Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var term = attributes["Term"];
        var qualifier = attributes.GetValueOrDefault("Qualifier");

        return new Annotation(term, qualifier);
    }
}
