namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM FunctionImport element.
/// </summary>
public sealed class FunctionImport : EdmElementBase, IModelElementFactory<FunctionImport>
{
    private FunctionImport(string name, string function, string? entitySet, bool includeInServiceDocument) : base(name)
    {
        Function = function;
        EntitySet = entitySet;
        IncludeInServiceDocument = includeInServiceDocument;
    }

    /// <summary>
    /// Gets the qualified unbound function name.
    /// </summary>
    public string Function { get; }

    /// <summary>
    /// Gets the entity set name or target path.
    /// </summary>
    public string? EntitySet { get; }

    /// <summary>
    /// Gets a value indicating whether this function import should be included in the service document.
    /// </summary>
    public bool IncludeInServiceDocument { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            yield return (nameof(Function), Function);
            if (EntitySet != null) yield return (nameof(EntitySet), EntitySet);
            if (!IncludeInServiceDocument) yield return (nameof(IncludeInServiceDocument), IncludeInServiceDocument);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => $".{Name}";

    /// <inheritdoc />
    public static FunctionImport Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var function = attributes["Function"];
        var entitySet = attributes.GetValueOrDefault("EntitySet");
        var includeInServiceDocument = bool.Parse(attributes.GetValueOrDefault("IncludeInServiceDocument", "true"));

        var functionImport = new FunctionImport(name, function, entitySet, includeInServiceDocument);

        // Handle Function reference resolution (qualified unbound function name)
        context.AddDeferredAction(new DeferredAction(functionImport, resolutionContext =>
        {
            var functionElement = resolutionContext.ResolveReference<Function>(function);
            // Function resolution is for validation purposes - we keep the string reference
        }), priority: 500);

        // Handle EntitySet reference resolution (relative to container)
        if (entitySet != null)
        {
            context.AddDeferredAction(new DeferredAction(functionImport, resolutionContext =>
            {
                var entitySetElement = resolutionContext.ResolveRelativeReference<EntitySet>(functionImport, entitySet);
                // EntitySet resolution is for validation purposes - we keep the string reference
            }), priority: 500);
        }

        return functionImport;
    }
}
