namespace Shared.Permissions;

/// <summary>
/// Interface for all resource types in the RBAC system.
/// Resources represent entities that can have permissions applied to them.
/// </summary>
public interface IResource
{
    /// <summary>
    /// Gets the string identifier of the resource
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Checks if the given action is valid for this resource
    /// </summary>
    bool IsValidAction(Action action);

    /// <summary>
    /// Gets all valid actions for this resource
    /// </summary>
    IReadOnlyList<Action> GetActions();
}

