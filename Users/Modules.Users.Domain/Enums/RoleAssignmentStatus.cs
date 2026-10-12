namespace Modules.Users.Domain.Enums;

public enum RoleAssignmentStatus
{
    /// <summary>The worker keeps trying to assign the role.</summary>
    Pending,

    /// <summary>Every attempt failed: the worker gave up. The user can only be deleted.</summary>
    Failed
}
