using System.Text.Json.Serialization;

namespace Modules.Users.Features.Features.Shared.Responses;

/// <param name="Role">
/// The user's app role (the role being assigned while <paramref name="Status"/> is not Active), or null when their
/// assignment has a role the application doesn't define.
/// </param>
public sealed record UserResponse(string Id, string DisplayName, string Email, string? Role, UserStatus Status);

[JsonConverter(typeof(JsonStringEnumConverter<UserStatus>))]
public enum UserStatus
{
    /// <summary>The role is assigned: the user can sign in.</summary>
    Active,

    /// <summary>Created; the role is being assigned in the background.</summary>
    Pending,

    /// <summary>The role could not be assigned: the user can only be deleted.</summary>
    Failed
}
