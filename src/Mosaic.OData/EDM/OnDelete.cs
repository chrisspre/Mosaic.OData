namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM OnDelete element.
/// </summary>
public sealed class OnDelete : EdmElementBase, IModelElementFactory<OnDelete>
{
    private OnDelete(string action) : base("OnDelete")
    {
        Action = action;
    }

    /// <summary>
    /// Gets the delete action (Cascade, None, SetNull, SetDefault).
    /// </summary>
    public string Action { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Action), Action);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // OnDelete is not resolvable via path navigation

    /// <inheritdoc />
    public static OnDelete Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var action = attributes["Action"];
        return new OnDelete(action);
    }
}
