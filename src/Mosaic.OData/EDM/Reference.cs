namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDMX Reference element.
/// </summary>
public sealed class Reference : EdmElement, IModelElementFactory<Reference>
{
    private Reference(string uri) : base("Reference")
    {
        Uri = uri;
    }

    /// <summary>
    /// Gets the URI of the referenced document.
    /// </summary>
    public string Uri { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Uri), Uri);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // Reference has no path segment

    /// <inheritdoc />
    public static Reference Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var uri = attributes["Uri"];
        return new Reference(uri);
    }
}
