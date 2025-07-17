namespace Mosaic.OData.EDM;

/// <summary>
/// Exception thrown when an EDM element cannot be created due to missing required attributes or invalid configuration.
/// This is a recoverable exception that should be caught during model building to allow partial model creation.
/// </summary>
public sealed class EdmElementCreationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EdmElementCreationException"/> class.
    /// </summary>
    /// <param name="elementType">The type of element that failed to be created.</param>
    /// <param name="message">The error message.</param>
    public EdmElementCreationException(string elementType, string message) 
        : base($"Failed to create {elementType}: {message}")
    {
        ElementType = elementType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EdmElementCreationException"/> class.
    /// </summary>
    /// <param name="elementType">The type of element that failed to be created.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EdmElementCreationException(string elementType, string message, Exception innerException) 
        : base($"Failed to create {elementType}: {message}", innerException)
    {
        ElementType = elementType;
    }

    /// <summary>
    /// Gets the type of element that failed to be created.
    /// </summary>
    public string ElementType { get; }
}
