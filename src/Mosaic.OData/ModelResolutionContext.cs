namespace Mosaic.OData.EDM;

/// <summary>
/// Context used during model resolution phase to resolve symbolic references and paths.
/// </summary>
public class ModelResolutionContext
{
    private readonly Dictionary<string, IEdmElement> _pathToElementMap = new();
    private readonly Schema _rootSchema;

    /// <summary>
    /// Initializes the resolution context with all resolvable elements from the model.
    /// </summary>
    /// <param name="rootSchema">The root schema of the model.</param>
    public ModelResolutionContext(Schema rootSchema)
    {
        _rootSchema = rootSchema;
        BuildPathMap(rootSchema, "");
    }

    /// <summary>
    /// Gets the root schema of the model.
    /// </summary>
    public Schema RootSchema => _rootSchema;

    /// <summary>
    /// Resolves a symbolic reference path to the target element.
    /// </summary>
    /// <typeparam name="T">The expected type of the target element.</typeparam>
    /// <param name="path">The path to resolve.</param>
    /// <returns>The resolved element of type T, or null if resolution fails.</returns>
    public T? ResolveReference<T>(string path) where T : class, IEdmElement
    {
        if (_pathToElementMap.TryGetValue(path, out var element) && element is T typedElement)
        {
            return typedElement;
        }

        Console.WriteLine($"Warning: Unable to resolve reference path '{path}' to element of type {typeof(T).Name}.");
        return null;
    }

    /// <summary>
    /// Resolves a path expression to a sequence of elements forming a path.
    /// </summary>
    /// <typeparam name="T">The expected type of the target element.</typeparam>
    /// <param name="pathExpression">The path expression to resolve.</param>
    /// <param name="contextElement">The element that provides the context for relative path resolution.</param>
    /// <returns>A Path object containing the resolved element sequence, or null if resolution fails.</returns>
    public Path<T>? ResolvePath<T>(string pathExpression, IEdmElement contextElement) where T : class, IEdmElement
    {
        // Parse the path expression and resolve each segment
        var segments = pathExpression.Split(new[] { '/', '.' }, StringSplitOptions.RemoveEmptyEntries);
        var elements = new List<IEdmElement>();
        var currentElement = contextElement;

        foreach (var segment in segments)
        {
            // Find the child element with the matching path segment
            var childElement = currentElement.Children.FirstOrDefault(c => c.TargetPathSegment?.EndsWith(segment) == true);
            if (childElement == null)
            {
                Console.WriteLine($"Warning: Unable to resolve path segment '{segment}' from element '{currentElement.Name}'.");
                return null;
            }
            
            elements.Add(childElement);
            currentElement = childElement;
        }

        if (elements.LastOrDefault() is not T targetElement)
        {
            Console.WriteLine($"Warning: Path '{pathExpression}' does not resolve to an element of type {typeof(T).Name}.");
            return null;
        }

        return new Path<T>(elements);
    }

    /// <summary>
    /// Tries to resolve a symbolic reference path to the target element.
    /// </summary>
    /// <typeparam name="T">The expected type of the target element.</typeparam>
    /// <param name="path">The path to resolve.</param>
    /// <param name="element">The resolved element if successful.</param>
    /// <returns>True if the path was resolved successfully, false otherwise.</returns>
    public bool TryResolveReference<T>(string path, out T? element) where T : class, IEdmElement
    {
        if (_pathToElementMap.TryGetValue(path, out var foundElement) && foundElement is T typedElement)
        {
            element = typedElement;
            return true;
        }

        element = null;
        return false;
    }

    /// <summary>
    /// Builds the path-to-element mapping for all resolvable elements in the model.
    /// </summary>
    private void BuildPathMap(IEdmElement element, string currentPath)
    {
        var segment = element.TargetPathSegment;
        if (segment != null)
        {
            var fullPath = currentPath + segment;
            _pathToElementMap[fullPath] = element;
            
            // Recursively map all children
            foreach (var child in element.Children)
            {
                BuildPathMap(child, fullPath);
            }
        }
        else
        {
            // For non-resolvable elements, still process their children
            foreach (var child in element.Children)
            {
                BuildPathMap(child, currentPath);
            }
        }
    }

    /// <summary>
    /// Finds an element by name within the children of a specified parent element.
    /// </summary>
    /// <typeparam name="T">The expected type of the element to find.</typeparam>
    /// <param name="parentElement">The parent element to search within.</param>
    /// <param name="elementName">The name of the element to find within the parent.</param>
    /// <returns>The found element of type T, or null if not found.</returns>
    public T? FindElementInParent<T>(IEdmElement parentElement, string elementName) where T : class, IEdmElement
    {
        var element = parentElement.Children.OfType<T>()
            .FirstOrDefault(child => child.Name == elementName);
            
        if (element == null)
        {
            Console.WriteLine($"Warning: Unable to find element '{elementName}' of type {typeof(T).Name} in parent '{parentElement.Name}'.");
            return null;
        }
        
        return element;
    }

    /// <summary>
    /// Finds an element by name within the children of a target type (by type name).
    /// </summary>
    /// <typeparam name="T">The expected type of the element to find.</typeparam>
    /// <param name="targetTypeName">The name of the type to search within.</param>
    /// <param name="elementName">The name of the element to find within that type.</param>
    /// <returns>The found element of type T, or null if not found.</returns>
    public T? FindElementInType<T>(string targetTypeName, string elementName) where T : class, IEdmElement
    {
        // Parse the target type name to remove Collection() wrapper if present
        var typeName = targetTypeName;
        if (typeName.StartsWith("Collection(") && typeName.EndsWith(")"))
        {
            typeName = typeName[11..^1]; // Remove "Collection(" and ")"
        }
        
        // Find the target type in the schema
        var targetType = _rootSchema.Children
            .FirstOrDefault(child => child.Name == typeName);
            
        if (targetType == null)
        {
            Console.WriteLine($"Warning: Unable to find target type '{typeName}'.");
            return null;
        }
        
        return FindElementInParent<T>(targetType, elementName);
    }

    /// <summary>
    /// Finds an element by walking up the parent chain to find a suitable container.
    /// Useful for resolving relative references within a schema or container.
    /// </summary>
    /// <typeparam name="T">The expected type of the element to find.</typeparam>
    /// <param name="contextElement">The element providing context for the search.</param>
    /// <param name="elementName">The name of the element to find.</param>
    /// <returns>The found element of type T, or null if not found.</returns>
    public T? FindElementInContext<T>(IEdmElement contextElement, string elementName) where T : class, IEdmElement
    {
        var current = contextElement;
        
        // Walk up the parent chain looking for the element
        while (current != null)
        {
            // Try to find the element in the current level
            var element = current.Children.OfType<T>()
                .FirstOrDefault(child => child.Name == elementName);
                
            if (element != null)
            {
                return element;
            }
            
            // Move up to the parent
            current = current.Parent;
        }
        
        Console.WriteLine($"Warning: Unable to find element '{elementName}' of type {typeof(T).Name} in context of '{contextElement.Name}'.");
        return null;
    }

    /// <summary>
    /// Resolves a relative path within a container (like EntityContainer for EntitySet references).
    /// </summary>
    /// <typeparam name="T">The expected type of the target element.</typeparam>
    /// <param name="contextElement">The element providing context (should be within the container).</param>
    /// <param name="targetName">The name of the target element to find.</param>
    /// <returns>The found element of type T, or null if not found.</returns>
    public T? ResolveRelativeReference<T>(IEdmElement contextElement, string targetName) where T : class, IEdmElement
    {
        // For entity set/singleton references, find the EntityContainer
        var container = contextElement;
        while (container != null && container is not EntityContainer)
        {
            container = container.Parent;
        }
        
        if (container is EntityContainer entityContainer)
        {
            return FindElementInParent<T>(entityContainer, targetName);
        }
        
        // Fall back to schema-level search
        return FindElementInContext<T>(contextElement, targetName);
    }
}
