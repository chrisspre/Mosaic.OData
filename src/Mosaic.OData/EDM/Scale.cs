using System.Globalization;

namespace Mosaic.OData.EDM;

/// <summary>
/// Represents the scale for decimal properties in OData.
/// Can be a non-negative integer or symbolic values: floating, variable.
/// </summary>
public readonly struct Scale : IParsable<Scale>, IFormattable, IEquatable<Scale>
{
    private readonly int? _value;
    private readonly ScaleKind _kind;

    private enum ScaleKind
    {
        Numeric,
        Floating,
        Variable
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Scale"/> struct with a numeric value.
    /// </summary>
    /// <param name="value">The non-negative integer value for scale.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is negative.</exception>
    public Scale(int value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Scale value must be non-negative.");
        }
        _value = value;
        _kind = ScaleKind.Numeric;
    }

    private Scale(ScaleKind kind)
    {
        _value = null;
        _kind = kind;
    }

    /// <summary>
    /// Gets a Scale instance representing the 'floating' symbolic value.
    /// </summary>
    public static Scale Floating => new(ScaleKind.Floating);

    /// <summary>
    /// Gets a Scale instance representing the 'variable' symbolic value.
    /// </summary>
    public static Scale Variable => new(ScaleKind.Variable);

    /// <summary>
    /// Gets a value indicating whether this is a numeric scale.
    /// </summary>
    public bool IsNumeric => _kind == ScaleKind.Numeric;

    /// <summary>
    /// Gets the numeric value if this is a numeric scale.
    /// </summary>
    public int? NumericValue => _value;

    /// <inheritdoc />
    public static Scale Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (TryParse(s, provider, out var result))
        {
            return result;
        }

        throw new FormatException($"'{s}' is not a valid Scale value.");
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out Scale result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        if (string.Equals(s, "floating", StringComparison.OrdinalIgnoreCase))
        {
            result = Floating;
            return true;
        }

        if (string.Equals(s, "variable", StringComparison.OrdinalIgnoreCase))
        {
            result = Variable;
            return true;
        }

        if (int.TryParse(s, NumberStyles.Integer, provider, out var value) && value >= 0)
        {
            result = new Scale(value);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return _kind switch
        {
            ScaleKind.Floating => "floating",
            ScaleKind.Variable => "variable",
            ScaleKind.Numeric => _value!.Value.ToString(format, formatProvider),
            _ => throw new InvalidOperationException("Unknown scale kind.")
        };
    }

    /// <inheritdoc />
    public override string ToString() => ToString(null, null);

    /// <inheritdoc />
    public bool Equals(Scale other)
    {
        return _kind == other._kind && _value == other._value;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Scale other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_kind, _value);
    }

    /// <summary>
    /// Determines whether two Scale instances are equal.
    /// </summary>
    public static bool operator ==(Scale left, Scale right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two Scale instances are not equal.
    /// </summary>
    public static bool operator !=(Scale left, Scale right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Implicitly converts an integer to a Scale.
    /// </summary>
    public static implicit operator Scale(int value)
    {
        return new Scale(value);
    }
}
