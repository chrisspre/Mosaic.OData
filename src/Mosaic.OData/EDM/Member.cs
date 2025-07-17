namespace Mosaic.OData.EDM;

/// <summary>
/// Represents an EDM Member element within an EnumType.
/// </summary>
public sealed class Member : EdmElementBase, IModelElementFactory<Member>
{
    private Member(string name, long? value) : base(name)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the integer value of this enum member.
    /// </summary>
    public long? Value { get; }

    /// <inheritdoc />
    public override IEnumerable<(string Name, object? Value)> BasicProperties
    {
        get
        {
            yield return (nameof(Name), Name);
            if (Value.HasValue) yield return (nameof(Value), Value.Value);
        }
    }

    /// <inheritdoc />
    public override string? TargetPathSegment => null; // Members are not resolvable via path navigation

    /// <inheritdoc />
    public static Member Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes)
    {
        var name = attributes["Name"];
        var value = attributes.ParseOrDefault<long>("Value") ;

        return new Member(name, value);
    }
}
