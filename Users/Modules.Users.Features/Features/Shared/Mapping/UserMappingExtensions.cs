using ClaimPartners.Users.Domain;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Enums;
using Modules.Users.Features.Features.Shared.Responses;

namespace Modules.Users.Features.Features.Shared.Mapping;

internal static class UserMappingExtensions
{
    internal static UserResponse MapToResponse(this PartnerUser user) =>
        new(user.Id, user.DisplayName, user.Email, user.Role, UserStatus.Active);

    internal static UserResponse MapToResponse(this PendingRoleAssignment assignment) =>
        new(
            assignment.UserId.ToString(),
            assignment.DisplayName,
            assignment.Email,
            assignment.Role,
            assignment.Status == RoleAssignmentStatus.Failed ? UserStatus.Failed : UserStatus.Pending);
}
