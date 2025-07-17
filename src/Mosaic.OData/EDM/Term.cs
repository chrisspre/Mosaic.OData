namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Term element.
/// </summary>
public sealed class Term : EdmElementBase, IModelElementFactory<Term>
{
    private Term? _baseTerm;

    private Term(string name, string type, bool nullable, string? defaultValue, string? baseTermReference, string? appliesTo) : base(name)
    {
        Type = type;
        Nullable = nullable;
        DefaultValue = defaultValue;
        BaseTermReference = baseTermReference;
        AppliesTo = appliesTo;
    }

    /// <summary>
    /// Gets the type of this term.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets a value indicating whether this term can be null.
    /// </summary>
    public bool Nullable { get; }

    /// <summary>
    /// Gets the default value of this term.
    /// </summary>
    public string? DefaultValue { get; }

    /// <summary>
    /// Gets the qualified name of the base term.
    /// </summary>
    public string? BaseTermReference { get; }

    /// <summary>
    /// Gets the base term that this term extends.
    /// </summary>
    public Term? BaseTerm => _baseTerm;

    /// <summary>
    /// Gets the whitespace-separated list of symbolic values indicating what this term applies to.
    /// </summary>
    public string? AppliesTo { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Type), Type);
            if (!Nullable) yield return (nameof(Nullable), Nullable);
            if (DefaultValue != null) yield return (nameof(DefaultValue), DefaultValue);
            if (BaseTermReference != null) yield return (nameof(BaseTermReference), BaseTermReference);
            if (AppliesTo != null) yield return (nameof(AppliesTo), AppliesTo);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <summary>
    /// Sets the base term. Should only be called during model resolution.
    /// </summary>
    internal void SetBaseTerm(Term baseTerm)
    {
        _baseTerm = baseTerm;
    }

    /// <inheritdoc />
    public static Term Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var type = attributes["Type"];
        var nullable = bool.Parse(attributes.GetValueOrDefault("Nullable", "true"));
        var defaultValue = attributes.GetValueOrDefault("DefaultValue");
        var baseTermReference = attributes.GetValueOrDefault("BaseTerm");
        var appliesTo = attributes.GetValueOrDefault("AppliesTo");

        var term = new Term(name, type, nullable, defaultValue, baseTermReference, appliesTo);

        // Handle BaseTerm reference resolution
        if (baseTermReference != null)
        {
            context.AddDeferredAction(new DeferredAction(term, resolutionContext =>
            {
                var baseTerm = resolutionContext.ResolveReference<Term>(baseTermReference);
                if (baseTerm != null)
                {
                    term.SetBaseTerm(baseTerm);
                }
            }), priority: 100); // Lower priority to ensure terms are created first
        }

        return term;
    }
}
