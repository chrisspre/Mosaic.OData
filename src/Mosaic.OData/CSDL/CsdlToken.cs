namespace Mosaic.OData.CSDL;



/// <summary>
/// Base class for all CSDL tokens representing discriminated union pattern.
/// </summary>
public abstract class CsdlToken
{
    private CsdlToken() { }

    /// <summary>
    /// Represents the start of a CSDL XML element.
    /// </summary>
    public sealed class Start(string elementName, IReadOnlyDictionary<string, string> attributes, bool isRootElement = false) : CsdlToken
    {
        public string ElementName { get; } = elementName ?? throw new ArgumentNullException(nameof(elementName));
        
        public IReadOnlyDictionary<string, string> Attributes { get; } = attributes ?? throw new ArgumentNullException(nameof(attributes));
        
        public bool IsRootElement { get; } = isRootElement;
    }

    /// <summary>
    /// Represents the end of a CSDL XML element.
    /// </summary>
    public sealed class End(string elementName) : CsdlToken
    {
        public string ElementName { get; } = elementName ?? throw new ArgumentNullException(nameof(elementName));
    }

    /// <summary>
    /// Represents an annotation expression in either attribute or element representation.
    /// </summary>
    public sealed class Expression(string expressionType, EDM.Expression value, bool wasAttribute = false, bool isInlineAttribute = false) : CsdlToken
    {
        public string ExpressionType { get; } = expressionType ?? throw new ArgumentNullException(nameof(expressionType));

        public EDM.Expression Value { get; } = value ?? throw new ArgumentNullException(nameof(value));
        
        /// <summary>
        /// Indicates whether this annotation expression was represented as an attribute in the original document.
        /// When true, preserves the original attribute representation for better round-trip fidelity.
        /// </summary>
        public bool WasAttribute { get; } = wasAttribute;
        
        /// <summary>
        /// Indicates whether this expression should be written as inline content (e.g., text content of an element).
        /// </summary>
        public bool IsInlineAttribute { get; } = isInlineAttribute;
    }
}
