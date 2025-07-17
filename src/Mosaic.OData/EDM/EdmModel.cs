using Mosaic.OData.CSDL;

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
                        if (parent is EdmElementBase parentBase && element is EdmElementBase elementBase)
                        {
                            parentBase.AddChild(elementBase);
                            elementBase.SetParent(parentBase);
                        }
                        
                        context.PushElement(element);
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
    /// Creates a model element based on the element name and attributes.
    /// </summary>
    private static IEdmElement? CreateModelElement(ModelBuilderContext context, string elementName, IReadOnlyDictionary<string, string> attributes)
    {
        return elementName switch
        {
            // EDMX wrapper elements
            "Edmx" => Edmx.Create(context, attributes),
            "DataServices" => DataServices.Create(context, attributes),
            "Reference" => Reference.Create(context, attributes),
            "Include" => Include.Create(context, attributes),
            
            // Core EDM elements
            "Schema" => Schema.Create(context, attributes),
            "EntityType" => EntityType.Create(context, attributes),
            "ComplexType" => ComplexType.Create(context, attributes),
            "EnumType" => EnumType.Create(context, attributes),
            "TypeDefinition" => TypeDefinition.Create(context, attributes),
            "Property" => Property.Create(context, attributes),
            "NavigationProperty" => NavigationProperty.Create(context, attributes),
            "Key" => Key.Create(context, attributes),
            "PropertyRef" => PropertyRef.Create(context, attributes),
            "ReferentialConstraint" => ReferentialConstraint.Create(context, attributes),
            "OnDelete" => OnDelete.Create(context, attributes),
            "Member" => Member.Create(context, attributes),
            "Action" => Action.Create(context, attributes),
            "Function" => Function.Create(context, attributes),
            "Parameter" => Parameter.Create(context, attributes),
            "ReturnType" => ReturnType.Create(context, attributes),
            "EntityContainer" => EntityContainer.Create(context, attributes),
            "EntitySet" => EntitySet.Create(context, attributes),
            "Singleton" => Singleton.Create(context, attributes),
            "ActionImport" => ActionImport.Create(context, attributes),
            "FunctionImport" => FunctionImport.Create(context, attributes),
            "NavigationPropertyBinding" => NavigationPropertyBinding.Create(context, attributes),
            "Term" => Term.Create(context, attributes),
            "Annotation" => Annotation.Create(context, attributes),
            "Annotations" => Annotations.Create(context, attributes),
            _ => null // Unknown elements are ignored
        };
    }
}
