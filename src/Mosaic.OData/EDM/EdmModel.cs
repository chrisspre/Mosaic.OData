using Mosaic.OData.CSDL;
using Serilog;

namespace Mosaic.OData.EDM;

/// <summary>
/// Root class for the EDM model that provides model loading functionality.
/// </summary>
public class EdmModel
{
    private readonly Schema _rootSchema;

    private EdmModel(Schema rootSchema)
    {
        _rootSchema = rootSchema;
    }

    /// <summary>
    /// Gets the root schema of the model.
    /// </summary>
    public Schema RootSchema => _rootSchema;

    /// <summary>
    /// Loads an EDM model from CSDL tokens produced by a CSDL reader.
    /// </summary>
    /// <param name="reader">The CSDL reader to read tokens from.</param>
    /// <returns>A fully constructed and resolved EDM model.</returns>
    public static EdmModel Load(CsdlXmlReader reader)
    {
        var context = new ModelBuilderContext();
        var tokens = reader.ReadTokens().ToList();
        
        // Phase 1: Build the model structure and collect deferred actions
        Schema? rootSchema = null;
        
        foreach (var token in tokens)
        {
            switch (token)
            {
                case CsdlToken.Start start:
                    try
                    {
                        var element = CreateModelElement(context, start.ElementName, start.Attributes);
                        if (element != null)
                        {
                            // Find the first Schema element regardless of whether it's marked as root
                            if (element is Schema schema && rootSchema == null)
                            {
                                rootSchema = schema;
                            }
                            
                            // Set parent-child relationships
                            var parent = context.CurrentParent;
                            if (parent is EdmElement parentElement)
                            {
                                parentElement.AddChild(element);
                            }
                            
                            context.PushElement(element);
                        }
                    }
                    catch (EdmElementCreationException ex)
                    {
                        Log.Warning("Skipping {ElementName} element - {Message}", start.ElementName, ex.Message);
                        // Continue processing other elements - this allows partial model building
                    }
                    break;
                    
                case CsdlToken.End end:
                    if (context.CurrentParent?.Name == end.ElementName)
                    {
                        context.PopElement();
                    } else {
                        // Log.Warning($"Mismatched end tag for {end.ElementName}. Expected {context.CurrentParent?.Name}.");
                        // throw new InvalidOperationException($"Mismatched end tag for {end.ElementName}. Expected {context.CurrentParent?.Name}.");
                    }
                    break;
                    
                case CsdlToken.Expression expression:
                    // Handle expression tokens if needed
                    break;
            }
        }

        if (rootSchema == null)
        {
            throw new InvalidOperationException("No root schema found in the CSDL document.");
        }

        // Phase 2: Resolve symbolic references using deferred actions
        var resolutionContext = new ModelResolutionContext(rootSchema);
        
        foreach (var deferredAction in context.GetDeferredActions())
        {
            deferredAction.Action(resolutionContext);
        }

        return new EdmModel(rootSchema);
    }

    /// <summary>
    /// Static dictionary mapping element names to their factory methods for better performance.
    /// </summary>
    private static readonly Dictionary<string, Func<ModelBuilderContext, IReadOnlyDictionary<string, string>, IEdmElement?>> _elementFactories = new()
    {
        // EDMX wrapper elements
        ["Edmx"] = Edmx.Create,
        ["DataServices"] = DataServices.Create,
        ["Reference"] = Reference.Create,
        ["Include"] = Include.Create,
        ["IncludeAnnotations"] = IncludeAnnotations.Create,
        
        // Core EDM elements
        ["Action"] = Action.Create,
        ["ActionImport"] = ActionImport.Create,
        ["Annotation"] = Annotation.Create,
        ["Annotations"] = Annotations.Create,
        ["ComplexType"] = ComplexType.Create,
        ["EntityContainer"] = EntityContainer.Create,
        ["EntitySet"] = EntitySet.Create,
        ["EntityType"] = EntityType.Create,
        ["EnumType"] = EnumType.Create,
        ["Function"] = Function.Create,
        ["FunctionImport"] = FunctionImport.Create,
        ["Key"] = Key.Create,
        ["Member"] = Member.Create,
        ["NavigationProperty"] = NavigationProperty.Create,
        ["NavigationPropertyBinding"] = NavigationPropertyBinding.Create,
        ["OnDelete"] = OnDelete.Create,
        ["Parameter"] = Parameter.Create,
        ["Property"] = Property.Create,
        ["PropertyRef"] = PropertyRef.Create,
        ["ReferentialConstraint"] = ReferentialConstraint.Create,
        ["ReturnType"] = ReturnType.Create,
        ["Schema"] = Schema.Create,
        ["Singleton"] = Singleton.Create,
        ["Term"] = Term.Create,
        ["TypeDefinition"] = TypeDefinition.Create,
    };

    /// <summary>
    /// Creates a model element based on the element name and attributes.
    /// </summary>
    private static IEdmElement? CreateModelElement(ModelBuilderContext context, string elementName, IReadOnlyDictionary<string, string> attributes)
    {
        return _elementFactories.TryGetValue(elementName, out var factory) 
            ? factory(context, attributes) 
            : null; // Unknown elements are ignored
    }
}
