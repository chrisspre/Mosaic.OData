namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDMX DataServices element.
/// </summary>
public sealed class DataServices : EdmElement, IModelElementFactory<DataServices>
{
    private DataServices() : base("DataServices")
    {
    }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            // DataServices has no attributes
            yield break;
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // DataServices has no path segment

    /// <inheritdoc />
    public static DataServices Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        return new DataServices();
    }
}
