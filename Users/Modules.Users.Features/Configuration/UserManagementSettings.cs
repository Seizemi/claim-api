namespace Modules.Users.Features.Configuration;

/// <summary>
/// The backend's own key of the <c>PartnerUsers</c> section. The ClaimPartners.Users package reads the same section
/// and ignores it.
/// </summary>
internal sealed class UserManagementSettings
{
    internal const string SectionName = "PartnerUsers";

    /// <summary>Roles a Supervisor may give to the users they create. Never <c>Supervisor</c>.</summary>
    public List<string> AssignableRoles { get; init; } = [];
}
