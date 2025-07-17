namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Key element.
/// </summary>
public sealed class Key : EdmElement, IModelElementFactory<Key>
{
    private Key() : base("Key")
    {
    }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            // Key element has no attributes
            yield break;
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // Key is not resolvable via path navigation

    /// <inheritdoc />
    public static Key Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        return new Key();
    }
}
