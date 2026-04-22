namespace Shared.Permissions;

/// <summary>
/// Base class for resource implementations.
/// Provides common functionality for resource types.
/// </summary>
public abstract class 
    ResourceBase : IResource
{
    private readonly List<Action> _actions;

    /// <summary>
    /// Creates a new resource with the specified name and actions
    /// </summary>
    protected ResourceBase(string name, params Action[] actions)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name cannot be null or empty", nameof(name));

        Name = name;
        _actions = new List<Action>(actions);
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public bool IsValidAction(Action action)
    {
        return _actions.Contains(action);
    }

    /// <inheritdoc />
    public IReadOnlyList<Action> GetActions()
    {
        return _actions.AsReadOnly();
    }

    /// <summary>
    /// Returns the string representation of the resource
    /// </summary>
    public override string ToString() => Name;
}

