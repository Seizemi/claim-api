using Modules.Users.Domain.Enums;

namespace Modules.Users.Domain.Entities;

/// <summary>
/// A user created in Entra whose app role is not assigned yet: Entra may take from seconds to over a minute before a
/// new user can be assigned, so the role is assigned in the background. The row is deleted once the role is assigned.
/// </summary>
public class PendingRoleAssignment
{
    /// <summary>Entra object ID of the created user.</summary>
    public Guid UserId { get; set; }

    public required string Email { get; set; }
    public required string DisplayName { get; set; }

    /// <summary>Role value to assign, e.g. Agent.</summary>
    public required string Role { get; set; }

    /// <summary>Object ID of the Supervisor who created the user.</summary>
    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public RoleAssignmentStatus Status { get; set; }

    /// <summary>Assignment attempts made so far.</summary>
    public int Attempts { get; set; }

    public DateTimeOffset NextAttemptAt { get; set; }

    /// <summary>Until when an instance of the worker owns the row; null when nobody does.</summary>
    public DateTimeOffset? LockedUntil { get; set; }

    /// <summary>Reason of the last failed attempt, without personal data.</summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Concurrency token, renewed on every update: two instances can never both take the same row.
    /// </summary>
    public Guid Version { get; set; }
}
