namespace Mosaic.OData.EDM;

/// <summary>
/// Interface for model elements that can be created from CSDL tokens using a factory pattern.
/// </summary>
/// <typeparam name="TSelf">The type of the model element that implements this interface.</typeparam>
public interface IModelElementFactory<TSelf> where TSelf : IEdmElement
{
    /// <summary>
    /// Creates a new instance of the model element from CSDL token attributes.
    /// </summary>
    /// <param name="context">The model builder context.</param>
    /// <param name="attributes">The attributes from the CSDL token.</param>
    /// <returns>A new instance of the model element.</returns>
    static abstract TSelf Create(ModelBuilderContext context, IReadOnlyDictionary<string, string> attributes);
}
