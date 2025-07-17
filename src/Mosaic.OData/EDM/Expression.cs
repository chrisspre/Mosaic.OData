namespace Mosaic.OData.EDM;

/// <summary>
/// Base class for annotation expressions representing discriminated union pattern.
/// </summary>
/// <remarks>
/// This class is used to represent various types of expressions in CSDL annotations.
/// see Expression entries under https://docs.oasis-open.org/odata/odata-csdl-xml/v4.02/csd01/odata-csdl-xml-v4.02-csd01.html#TableofXMLElementsandAttributes
///  This includes simple literals, paths, collections, records, and more.
/// </remarks>
public abstract class Expression
{
    private Expression() { }

    /// <summary>
    /// Returns a compact string representation of the expression.
    /// </summary>
    public abstract override string ToString();

    /// <summary>
    /// Represents a simple literal value expression (String, Int, Bool, etc.).
    /// </summary>
    public sealed class Literal(string value) : Expression
    {
        public string Value { get; } = value ?? throw new ArgumentNullException(nameof(value));
        
        public override string ToString() => Value;
    }

    /// <summary>
    /// Represents a path expression (Path, PropertyPath, NavigationPropertyPath, AnnotationPath).
    /// </summary>
    public sealed class Path(string path) : Expression
    {
        public string PathValue { get; } = path ?? throw new ArgumentNullException(nameof(path));
        
        public override string ToString() => $"@{PathValue}";
    }

    /// <summary>
    /// Represents a null expression.
    /// </summary>
    public sealed class Null : Expression
    {
        public static readonly Null Instance = new();
        private Null() { }
        
        public override string ToString() => "null";
    }

    /// <summary>
    /// Represents a collection expression containing multiple child expressions.
    /// </summary>
    public sealed class Collection(IEnumerable<Expression> items) : Expression
    {
        public IReadOnlyList<Expression> Items { get; } = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        
        public override string ToString() => $"[{string.Join(", ", Items)}]";
    }

    /// <summary>
    /// Represents a record expression with properties and optional type.
    /// </summary>
    public sealed class Record(IEnumerable<PropertyValue> properties, string? type = null) : Expression
    {
        public IReadOnlyList<PropertyValue> Properties { get; } = properties?.ToList() ?? throw new ArgumentNullException(nameof(properties));
        public string? Type { get; } = type;
        
        public override string ToString()
        {
            var props = string.Join(", ", Properties);
            return Type != null ? $"{Type}{{{props}}}" : $"{{{props}}}";
        }
    }

    /// <summary>
    /// Represents a property value within a record expression.
    /// </summary>
    public sealed class PropertyValue(string property, Expression value) : Expression
    {
        public string Property { get; } = property ?? throw new ArgumentNullException(nameof(property));
        public Expression Value { get; } = value ?? throw new ArgumentNullException(nameof(value));
        
        public override string ToString() => $"{Property}: {Value}";
    }

    /// <summary>
    /// Represents an apply expression with function and arguments.
    /// </summary>
    public sealed class Apply(string function, IEnumerable<Expression> arguments) : Expression
    {
        public string Function { get; } = function ?? throw new ArgumentNullException(nameof(function));
        public IReadOnlyList<Expression> Arguments { get; } = arguments?.ToList() ?? throw new ArgumentNullException(nameof(arguments));
        
        public override string ToString() => $"{Function}({string.Join(", ", Arguments)})";
    }

    /// <summary>
    /// Represents a logical expression (And, Or, Not, etc.).
    /// </summary>
    public sealed class Logical(string operation, IEnumerable<Expression> operands) : Expression
    {
        public string Operation { get; } = operation ?? throw new ArgumentNullException(nameof(operation));
        public IReadOnlyList<Expression> Operands { get; } = operands?.ToList() ?? throw new ArgumentNullException(nameof(operands));
        
        public override string ToString()
        {
            if (Operation.Equals("Not", StringComparison.OrdinalIgnoreCase) && Operands.Count == 1)
                return $"!{Operands[0]}";
            
            var op = Operation.ToLowerInvariant() switch
            {
                "and" => " && ",
                "or" => " || ",
                _ => $" {Operation} "
            };
            return $"({string.Join(op, Operands)})";
        }
    }

    /// <summary>
    /// Represents a comparison expression (Eq, Ne, Gt, Lt, etc.).
    /// </summary>
    public sealed class Comparison(string operation, Expression left, Expression right) : Expression
    {
        public string Operation { get; } = operation ?? throw new ArgumentNullException(nameof(operation));
        public Expression Left { get; } = left ?? throw new ArgumentNullException(nameof(left));
        public Expression Right { get; } = right ?? throw new ArgumentNullException(nameof(right));
        
        public override string ToString()
        {
            var op = Operation.ToLowerInvariant() switch
            {
                "eq" => " == ",
                "ne" => " != ",
                "gt" => " > ",
                "ge" => " >= ",
                "lt" => " < ",
                "le" => " <= ",
                _ => $" {Operation} "
            };
            return $"{Left}{op}{Right}";
        }
    }

    /// <summary>
    /// Represents an arithmetic expression (Add, Sub, Mul, Div, etc.).
    /// </summary>
    public sealed class Arithmetic(string operation, Expression left, Expression right) : Expression
    {
        public string Operation { get; } = operation ?? throw new ArgumentNullException(nameof(operation));
        public Expression Left { get; } = left ?? throw new ArgumentNullException(nameof(left));
        public Expression Right { get; } = right ?? throw new ArgumentNullException(nameof(right));
        
        public override string ToString()
        {
            var op = Operation.ToLowerInvariant() switch
            {
                "add" => " + ",
                "sub" => " - ",
                "mul" => " * ",
                "div" => " / ",
                "mod" => " % ",
                _ => $" {Operation} "
            };
            return $"({Left}{op}{Right})";
        }
    }

    /// <summary>
    /// Represents a conditional (If) expression.
    /// </summary>
    public sealed class Conditional(Expression condition, Expression trueExpression, Expression falseExpression) : Expression
    {
        public Expression Condition { get; } = condition ?? throw new ArgumentNullException(nameof(condition));
        public Expression TrueExpression { get; } = trueExpression ?? throw new ArgumentNullException(nameof(trueExpression));
        public Expression FalseExpression { get; } = falseExpression ?? throw new ArgumentNullException(nameof(falseExpression));
        
        public override string ToString() => $"{Condition} ? {TrueExpression} : {FalseExpression}";
    }

    /// <summary>
    /// Represents a cast or type check expression (Cast, IsOf).
    /// </summary>
    public sealed class TypeOperation(string operation, Expression operand, string targetType) : Expression
    {
        public string Operation { get; } = operation ?? throw new ArgumentNullException(nameof(operation));
        public Expression Operand { get; } = operand ?? throw new ArgumentNullException(nameof(operand));
        public string TargetType { get; } = targetType ?? throw new ArgumentNullException(nameof(targetType));
        
        public override string ToString()
        {
            return Operation.Equals("Cast", StringComparison.OrdinalIgnoreCase)
                ? $"({TargetType}){Operand}"
                : $"{Operand} is {TargetType}";
        }
    }

    /// <summary>
    /// Represents a labeled element expression.
    /// </summary>
    public sealed class LabeledElement(string name, Expression expression) : Expression
    {
        public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
        public Expression InnerExpression { get; } = expression ?? throw new ArgumentNullException(nameof(expression));
        
        public override string ToString() => $"{Name}:{InnerExpression}";
    }

    /// <summary>
    /// Represents a URL reference expression.
    /// </summary>
    public sealed class UrlRef(string url) : Expression
    {
        public string Url { get; } = url ?? throw new ArgumentNullException(nameof(url));
        
        public override string ToString() => $"url({Url})";
    }
}
