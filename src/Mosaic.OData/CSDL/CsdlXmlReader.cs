namespace Mosaic.OData.CSDL;

using System.Xml;
using Mosaic.OData.EDM;

/// <summary>
/// Reads CSDL XML files and produces a stream of tokens.
/// </summary>
public class CsdlXmlReader(XmlReader xmlReader) : IDisposable
{
    private readonly XmlReader _xmlReader = xmlReader;
    private readonly Dictionary<string, string> _namespaces = [];
    private readonly Stack<string> _elementStack = new();

    public static CsdlXmlReader Create(string filePath)
    {
       
        var xmlReader = XmlReader.Create(filePath, XmlReaderSettings);
        return new CsdlXmlReader(xmlReader);
    }

    private static readonly XmlReaderSettings XmlReaderSettings = new()
    {
        IgnoreWhitespace = true,
        IgnoreComments = true
    };

    public static CsdlXmlReader Load(string content)
    {
        var stringReader = new StringReader(content);
        var xmlReader = XmlReader.Create(stringReader, XmlReaderSettings);
        return new CsdlXmlReader(xmlReader);
    }



    // public CsdlXmlReader(TextReader textReader)
    // {
    //     var settings = new XmlReaderSettings
    //     {
    //         IgnoreWhitespace = true,
    //         IgnoreComments = true
    //     };
    //     _xmlReader = XmlReader.Create(textReader, settings);
    // }

    private string GetLocalName()
    {
        return _xmlReader.LocalName;
    }

    /// <summary>
    /// Reads the CSDL XML and returns an enumerable of tokens.
    /// </summary>
    public IEnumerable<CsdlToken> ReadTokens()
    {
        while (_xmlReader.Read())
        {
            switch (_xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    var elementName = GetLocalName();
                    var attributes = ReadAttributes();

                    // Check if this is an annotation expression element (like Collection, Record, Apply, etc.)
                    if (IsAnnotationExpressionElement(elementName))
                    {
                        // Parse the entire annotation expression and emit it as a single token
                        var expressionValue = ParseAnnotationExpressionContent(elementName, attributes);
                        yield return new CsdlToken.Expression(elementName, expressionValue, wasAttribute: false);
                    }
                    else
                    {
                        _elementStack.Push(elementName);

                        // Check for annotation expressions in attribute form
                        var annotationExpressions = new List<CsdlToken.Expression>();
                        var cleanAttributes = new Dictionary<string, string>();

                        foreach (var (key, value) in attributes)
                        {
                            if (IsAnnotationExpressionAttribute(key))
                            {
                                // This is an annotation expression in attribute form
                                var literalExpression = new Expression.Literal(value);
                                annotationExpressions.Add(new CsdlToken.Expression(key, literalExpression, wasAttribute: true));
                            }
                            else
                            {
                                cleanAttributes[key] = value;
                            }
                        }

                        yield return new CsdlToken.Start(elementName, cleanAttributes);

                        // Emit annotation expression tokens for attributes
                        foreach (var expr in annotationExpressions)
                        {
                            yield return expr;
                        }

                        if (_xmlReader.IsEmptyElement)
                        {
                            _elementStack.Pop();
                            yield return new CsdlToken.End(elementName);
                        }
                    }
                    break;

                case XmlNodeType.EndElement:
                    var endElementName = GetLocalName();
                    if (_elementStack.Count > 0 && _elementStack.Peek() == endElementName)
                    {
                        _elementStack.Pop();
                    }
                    yield return new CsdlToken.End(endElementName);
                    break;

                case XmlNodeType.Text:
                    // Capture text content for annotation expression elements
                    if (_elementStack.Count > 0)
                    {
                        var parentElementName = _elementStack.Peek();
                        if (IsAnnotationExpressionElement(parentElementName))
                        {
                            var literalExpression = new Expression.Literal(_xmlReader.Value ?? "");
                            yield return new CsdlToken.Expression(parentElementName, literalExpression, wasAttribute: false);
                        }
                    }
                    break;
            }
        }
    }

    private Dictionary<string, string> ReadAttributes()
    {
        var attributes = new Dictionary<string, string>();

        if (_xmlReader.HasAttributes)
        {
            for (var i = 0; i < _xmlReader.AttributeCount; i++)
            {
                _xmlReader.MoveToAttribute(i);

                // Handle namespace declarations
                if (_xmlReader.Name == "xmlns" || _xmlReader.Name.StartsWith("xmlns:"))
                {
                    var prefix = _xmlReader.Name == "xmlns" ? "" : _xmlReader.Name[6..];
                    _namespaces[prefix] = _xmlReader.Value;
                }
                else
                {
                    attributes[_xmlReader.Name] = _xmlReader.Value;
                }
            }
            _xmlReader.MoveToElement();
        }

        return attributes;
    }

    private static readonly HashSet<string> AnnotationExpressionTypes =
    [
        "Binary", "Bool", "Date", "DateTimeOffset", "Decimal", "Duration", "Float", "Guid", "Int",
        "String", "TimeOfDay", "And", "Or", "Not", "Eq", "Ne", "Ge", "Gt", "Le", "Lt", "Has",
        "In", "Add", "Sub", "Neg", "Mul", "Div", "DivBy", "Mod", "Cast", "IsOf", "LabeledElement",
        "Collection", "Record", "If", "Apply", "Null", "Path", "PropertyPath", "NavigationPropertyPath",
        "AnnotationPath", "UrlRef"
    ];

    private static bool IsAnnotationExpressionAttribute(string attributeName)
    {
        return AnnotationExpressionTypes.Contains(attributeName);
    }

    private static bool IsAnnotationExpressionElement(string elementName)
    {
        return AnnotationExpressionTypes.Contains(elementName);
    }

    private Expression ParseAnnotationExpressionContent(string expressionType, Dictionary<string, string> attributes)
    {
        // Handle different expression types
        return expressionType switch
        {
            // Simple literal expressions
            "String" or "Binary" or "Bool" or "Date" or "DateTimeOffset" or "Decimal" or "Duration"
            or "Float" or "Guid" or "Int" or "TimeOfDay" => ParseLiteralExpression(),

            // Path expressions
            "Path" or "PropertyPath" or "NavigationPropertyPath" or "AnnotationPath" => ParsePathExpression(),

            // Null expression
            "Null" => Expression.Null.Instance,

            // Collection expression
            "Collection" => ParseCollectionExpression(),

            // Record expression
            "Record" => ParseRecordExpression(attributes),

            // Apply expression
            "Apply" => ParseApplyExpression(attributes),

            // Logical expressions
            "And" or "Or" or "Not" => ParseLogicalExpression(expressionType),

            // Comparison expressions
            "Eq" or "Ne" or "Ge" or "Gt" or "Le" or "Lt" or "Has" or "In" => ParseComparisonExpression(expressionType),

            // Arithmetic expressions
            "Add" or "Sub" or "Neg" or "Mul" or "Div" or "DivBy" or "Mod" => ParseArithmeticExpression(expressionType),

            // Type operations
            "Cast" or "IsOf" => ParseTypeOperationExpression(expressionType, attributes),

            // Conditional expression
            "If" => ParseConditionalExpression(),

            // Labeled element
            "LabeledElement" => ParseLabeledElementExpression(attributes),

            // URL reference
            "UrlRef" => ParseUrlRefExpression(),

            // Default case - treat as literal
            _ => ParseLiteralExpression()
        };
    }

    private Expression ParseLiteralExpression()
    {
        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Literal("");
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Text)
            {
                var textValue = _xmlReader.Value?.Trim() ?? "";
                return new Expression.Literal(textValue);
            }
        }

        return new Expression.Literal("");
    }

    private Expression ParsePathExpression()
    {
        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Path("");
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Text)
            {
                var pathValue = _xmlReader.Value?.Trim() ?? "";
                return new Expression.Path(pathValue);
            }
        }

        return new Expression.Path("");
    }

    private Expression ParseCollectionExpression()
    {
        var items = new List<Expression>();

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Collection(items);
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (IsAnnotationExpressionElement(childElementName))
                {
                    var childExpression = ParseAnnotationExpressionContent(childElementName, childAttributes);
                    items.Add(childExpression);
                }
            }
        }

        return new Expression.Collection(items);
    }

    private Expression ParseRecordExpression(Dictionary<string, string> attributes)
    {
        var properties = new List<Expression.PropertyValue>();
        var recordType = attributes.TryGetValue("Type", out var type) ? type : null;

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Record(properties, recordType);
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (childElementName == "PropertyValue")
                {
                    var propertyName = childAttributes.TryGetValue("Property", out var prop) ? prop : "";

                    // PropertyValue can have inline value or child expression
                    Expression propertyExpression;

                    if (_xmlReader.IsEmptyElement)
                    {
                        // Check for Value attribute for inline values
                        if (childAttributes.TryGetValue("Value", out var inlineValue))
                        {
                            propertyExpression = new Expression.Literal(inlineValue);
                        }
                        else
                        {
                            propertyExpression = new Expression.Literal("");
                        }
                    }
                    else
                    {
                        // Parse child expression content
                        var childDepth = _xmlReader.Depth;
                        propertyExpression = new Expression.Literal(""); // Default

                        while (_xmlReader.Read() && _xmlReader.Depth > childDepth)
                        {
                            if (_xmlReader.NodeType == XmlNodeType.Element)
                            {
                                var exprElementName = GetLocalName();
                                var exprAttributes = ReadAttributes();

                                if (IsAnnotationExpressionElement(exprElementName))
                                {
                                    propertyExpression = ParseAnnotationExpressionContent(exprElementName, exprAttributes);
                                    break; // First child expression wins
                                }
                            }
                            else if (_xmlReader.NodeType == XmlNodeType.Text)
                            {
                                var textValue = _xmlReader.Value?.Trim();
                                if (!string.IsNullOrEmpty(textValue))
                                {
                                    propertyExpression = new Expression.Literal(textValue);
                                    break;
                                }
                            }
                        }
                    }

                    properties.Add(new Expression.PropertyValue(propertyName, propertyExpression));
                }
            }
        }

        return new Expression.Record(properties, recordType);
    }

    private Expression ParseApplyExpression(Dictionary<string, string> attributes)
    {
        var function = attributes.TryGetValue("Function", out var func) ? func : "";
        var arguments = new List<Expression>();

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Apply(function, arguments);
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (IsAnnotationExpressionElement(childElementName))
                {
                    var childExpression = ParseAnnotationExpressionContent(childElementName, childAttributes);
                    arguments.Add(childExpression);
                }
            }
        }

        return new Expression.Apply(function, arguments);
    }

    private Expression ParseLogicalExpression(string operation)
    {
        var operands = new List<Expression>();

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Logical(operation, operands);
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (IsAnnotationExpressionElement(childElementName))
                {
                    var childExpression = ParseAnnotationExpressionContent(childElementName, childAttributes);
                    operands.Add(childExpression);
                }
            }
        }

        return new Expression.Logical(operation, operands);
    }

    private Expression ParseComparisonExpression(string operation)
    {
        var operands = new List<Expression>();

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Logical(operation, operands);
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (IsAnnotationExpressionElement(childElementName))
                {
                    var childExpression = ParseAnnotationExpressionContent(childElementName, childAttributes);
                    operands.Add(childExpression);
                }
            }
        }

        // For comparison, we expect exactly 2 operands
        if (operands.Count >= 2)
        {
            return new Expression.Comparison(operation, operands[0], operands[1]);
        }

        // Fallback to logical for cases with != 2 operands
        return new Expression.Logical(operation, operands);
    }

    private Expression ParseArithmeticExpression(string operation)
    {
        var operands = new List<Expression>();

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Logical(operation, operands);
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (IsAnnotationExpressionElement(childElementName))
                {
                    var childExpression = ParseAnnotationExpressionContent(childElementName, childAttributes);
                    operands.Add(childExpression);
                }
            }
        }

        // For arithmetic, we expect exactly 2 operands (except Neg which has 1)
        if (operation == "Neg" && operands.Count >= 1)
        {
            return new Expression.Arithmetic(operation, operands[0], new Expression.Literal("0"));
        }
        if (operands.Count >= 2)
        {
            return new Expression.Arithmetic(operation, operands[0], operands[1]);
        }

        // Fallback to logical for cases with != 2 operands
        return new Expression.Logical(operation, operands);
    }

    private Expression ParseTypeOperationExpression(string operation, Dictionary<string, string> attributes)
    {
        var targetType = attributes.TryGetValue("Type", out var type) ? type : "";

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.TypeOperation(operation, new Expression.Literal(""), targetType);
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (IsAnnotationExpressionElement(childElementName))
                {
                    var operand = ParseAnnotationExpressionContent(childElementName, childAttributes);
                    return new Expression.TypeOperation(operation, operand, targetType);
                }
            }
        }

        return new Expression.TypeOperation(operation, new Expression.Literal(""), targetType);
    }

    private Expression ParseConditionalExpression()
    {
        var expressions = new List<Expression>();

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.Conditional(
                new Expression.Literal(""),
                new Expression.Literal(""),
                new Expression.Literal(""));
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (IsAnnotationExpressionElement(childElementName))
                {
                    var childExpression = ParseAnnotationExpressionContent(childElementName, childAttributes);
                    expressions.Add(childExpression);
                }
            }
        }

        // If expects 3 expressions: condition, true, false
        var condition = expressions.Count > 0 ? expressions[0] : new Expression.Literal("");
        var trueExpr = expressions.Count > 1 ? expressions[1] : new Expression.Literal("");
        var falseExpr = expressions.Count > 2 ? expressions[2] : new Expression.Literal("");

        return new Expression.Conditional(condition, trueExpr, falseExpr);
    }

    private Expression ParseLabeledElementExpression(Dictionary<string, string> attributes)
    {
        var name = attributes.TryGetValue("Name", out var labelName) ? labelName : "";

        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.LabeledElement(name, new Expression.Literal(""));
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Element)
            {
                var childElementName = GetLocalName();
                var childAttributes = ReadAttributes();

                if (IsAnnotationExpressionElement(childElementName))
                {
                    var childExpression = ParseAnnotationExpressionContent(childElementName, childAttributes);
                    return new Expression.LabeledElement(name, childExpression);
                }
            }
        }

        return new Expression.LabeledElement(name, new Expression.Literal(""));
    }

    private Expression ParseUrlRefExpression()
    {
        if (_xmlReader.IsEmptyElement)
        {
            return new Expression.UrlRef("");
        }

        var depth = _xmlReader.Depth;
        while (_xmlReader.Read() && _xmlReader.Depth > depth)
        {
            if (_xmlReader.NodeType == XmlNodeType.Text)
            {
                var url = _xmlReader.Value?.Trim() ?? "";
                return new Expression.UrlRef(url);
            }
        }

        return new Expression.UrlRef("");
    }

    public void Dispose()
    {
        _xmlReader?.Dispose();
    }
}
