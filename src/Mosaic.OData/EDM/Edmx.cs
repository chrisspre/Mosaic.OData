namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDMX Edmx root element.
/// </summary>
public sealed class Edmx : EdmElement, IModelElementFactory<Edmx>
{
    private Edmx(string version) : base("Edmx")
    {
        Version = version;
    }

    /// <summary>
    /// Gets the version of the EDMX document.
    /// </summary>
    public string Version { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Version), Version);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // EDMX root has no path segment

    /// <inheritdoc />
    public static Edmx Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var version = attributes.GetValueOrDefault("Version", "4.0");
        return new Edmx(version);
    }
}
