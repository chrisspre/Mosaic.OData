namespace Mosaic.OData.EDM;

/// <summary>
/// Context used during model building phase to track state and queue deferred actions.
/// </summary>
public class ModelBuilderContext
{
    private readonly Stack<IEdmElement> _elementStack = new();
    private readonly PriorityQueue<DeferredAction, int> _deferredActions = new();

    /// <summary>
    /// Gets the current parent element from the element stack.
    /// </summary>
    public IEdmElement? CurrentParent => _elementStack.Count > 0 ? _elementStack.Peek() : null;

    /// <summary>
    /// Pushes an element onto the element stack, making it the current parent.
    /// </summary>
    /// <param name="element">The element to push onto the stack.</param>
    public void PushElement(IEdmElement element)
    {
        _elementStack.Push(element);
    }

    /// <summary>
    /// Pops the current element from the element stack.
    /// </summary>
    /// <returns>The element that was popped from the stack.</returns>
    public IEdmElement PopElement()
    {
        return _elementStack.Pop();
    }

    /// <summary>
    /// Adds a deferred action to be executed later during model resolution.
    /// </summary>
    /// <param name="action">The action to defer.</param>
    /// <param name="priority">The priority of the action (lower numbers execute first).</param>
    public void AddDeferredAction(DeferredAction action, int priority = 1000)
    {
        _deferredActions.Enqueue(action, priority);
    }

    /// <summary>
    /// Gets all deferred actions in priority order.
    /// </summary>
    public IEnumerable<DeferredAction> GetDeferredActions()
    {
        while (_deferredActions.Count > 0)
        {
            yield return _deferredActions.Dequeue();
        }
    }
}

/// <summary>
/// Represents a deferred action that will be executed during model resolution.
/// </summary>
/// <param name="Element">The element that the action applies to.</param>
/// <param name="Action">The action to execute.</param>
public record DeferredAction(IEdmElement Element, Action<ModelResolutionContext> Action);
