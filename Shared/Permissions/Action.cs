namespace Shared.Permissions;

/// <summary>
/// Represents an action that can be performed on a resource in the RBAC system.
/// Actions are type-safe strings that define what operations can be performed.
/// </summary>
public readonly record struct Action
{
    private readonly string _value;

    /// <summary>
    /// Creates a new Action with the specified value
    /// </summary>
    public Action(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Action value cannot be null or empty", nameof(value));
        
        _value = value;
    }

    /// <summary>
    /// Gets the string representation of the action
    /// </summary>
    public override string ToString() => _value;

    /// <summary>
    /// Implicit conversion from string to Action
    /// </summary>
    public static implicit operator string(Action action) => action._value;

    /// <summary>
    /// Implicit conversion from string to Action
    /// </summary>
    public static implicit operator Action(string value) => new(value);
}

