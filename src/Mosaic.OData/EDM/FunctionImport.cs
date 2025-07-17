namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM FunctionImport element.
/// </summary>
public sealed class FunctionImport : EdmElement, IModelElementFactory<FunctionImport>
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
        var name = attributes.GetRequiredOrDefault("Name", $"<MissingName_{Guid.NewGuid():N}>");
        var function = attributes.GetRequiredOrDefault("Function", "<MissingFunction>");
        var entitySet = attributes.GetValueOrDefault("EntitySet");
        var includeInServiceDocument = attributes.ParseOrDefault("IncludeInServiceDocument", true);

        var functionImport = new FunctionImport(name, function, entitySet, includeInServiceDocument);

        // Handle Function reference resolution (qualified unbound function name)
        context.AddDeferredAction(500, functionImport, resolutionContext =>
        {
            var functionElement = resolutionContext.ResolveReference<Function>(function);
            // Function resolution is for validation purposes - we keep the string reference
        });

        // Handle EntitySet reference resolution (relative to container)
        if (entitySet != null)
        {
            context.AddDeferredAction(500, functionImport, resolutionContext =>
            {
                var entitySetElement = resolutionContext.ResolveRelativeReference<EntitySet>(functionImport, entitySet);
                // EntitySet resolution is for validation purposes - we keep the string reference
            });
        }

        return functionImport;
    }
}
