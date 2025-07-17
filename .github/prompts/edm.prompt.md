---
mode: agent
---

Let's create the EDM model, a strongly typed and minimally pplomorphic object graph that represents the EDM structure, including entities, complex types, and their relationships. The model should be built using the tokens produced by the CSDL reader.

The implementation should follow the structure in src\Mosaic.OData\EDM_Structure.md, i.e. implement
one class per model elements with strongly types properties accorting the the categorization of attributes in the document.

Each c# class for a model element should be 
- immutable,
- in a separate file, 
- with the class name matching the element name (e.g., `EntityType.cs` for `<EntityType>` elements). 
- following the structure in  src\Mosaic.OData\EDM_Structure.md
    - implement strongly typed properties for all *basic attributes* 
    - implement strongly typed properties for all *reference attributes* that reference the related element .OData\EDM_Structure.md 
    - implement strongly typed properties for all *path attributes* of type Path<T> where T is the type of the referenced element and path is a non empty list of elements that build a sequence endding in a T. 



Additional each model element class should implement an interface IEdmElement (probably best as an explicit interface implementation) that includes:
- a Name property of type string,
- a Parent property of type IEdmElement (non-nullable),
- a Children property of type IReadOnlyList<IEdmElement> (non-nullable),
- a BasicProperties property of type IEnumerable<(string, object?)> (non-nullable)
- a TargetPathSegment property of type string (nullable) that returns the path segment of the element, which is used to resolve symbolic references. the TargetPathSegment must include the preceding path separator (e.g. an Entity returns $".{name}" and a Property returns $"/{name}", Schema has no separator). Unresolvable elements (like a PropertyRef) return null for the segment to indicate that they are not resolvable.


Each class should have a ToString method that creates a strinng of the element as as an empty XML element with jsut the basic properties, no parent/child elements, no references, just the basic attributes.



Last but not least create a EdmModel.Load method that leverages the CSDL reader and
constructs the model graph from the tokens produced by the reader. 

In support of EdmModel.Load there should be a IModelElementFactory<TSelf> interface with a static abstract `TSelf Create(ModelBuilderContext ctx, IDictionary<string,string> attributes`) method implemented by each model element class. The `Create` method: 
- takes returns a new instance of the class with all basic properties set based on the token attributes passed as a parameter. The  deferred actions are closures that
    - receive the typed model element (hence `Action<T, ModelResolutionContext>` where T is the type of the model element, and ModelResolutionContext is a context that allows to resolve symbolic references)
    - closes over the attribute values that are symbolic references and 
    - receives a context parameter that allows to resolve the symbolic references (including path artrributes) captured in the closure.
    - since Create it is a method on the ModelElement, it should use private setter properties to store references to the resolved elements. 


The EdmModel.Load method should works in 2 phases:
- Phase 1: 
    - Read all tokens and build the initial tree structure of the model, including the the parent-child relationships using a stack of elements to track the current parent and setting parent and children properties accordingly.
    - it should construct the model element via a static IModelElementFactory.Create method 
    - the deferred actions should be stored in a priority queue of actions that will be executed later in priority order    
    - the the symbolic references are calculated by traversing the elements up parent chain and using the TargetPathSegment method mentioned above asking each element about their "Segment" of the path. The Segment needs to include the path separator (e.g. an Entity returns $".{name}" and a property returns $".{name}". unresolveable elemtents (like a ropertyRef) return null for the segment to indicate that they are not resolvable.

- Phase 2: 
    - execute all the defered actions buy providing a the ModelResolutionContext that can find an element based on its path (constructed from its own and its ancestors TargetPathSegment)     
    - the deferred actions need a priority so that for example EDM Property Types are all set before more involved references are to be resolved.
    - PAth attributes dould be resolved in a simlar way via a method on the ModelResolutionContext that resolves a path to a sequence of elements, where each element is a child of the type of the previous element in the chain.


