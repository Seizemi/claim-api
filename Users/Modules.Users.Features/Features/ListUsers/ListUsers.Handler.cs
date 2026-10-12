using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Users.Features.Abstractions;
using Modules.Users.Features.Features.Shared.Errors;
using Modules.Users.Features.Features.Shared.Mapping;
using Modules.Users.Features.Features.Shared.Responses;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Features.ListUsers;

internal interface IListUsersHandler : IHandler
{
    Task<ErrorOr<IReadOnlyList<UserResponse>>> HandleAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Users assigned to the application (Active) plus the users whose role is still being assigned (Pending) or could
/// not be (Failed), so that every user created from the application is visible.
/// </summary>
internal sealed class ListUsersHandler(
    IPartnerUserService partnerUsers,
    UsersDbContext dbContext,
    ILogger<ListUsersHandler> logger) : IListUsersHandler
{
    public async Task<ErrorOr<IReadOnlyList<UserResponse>>> HandleAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<PartnerUser> users;
        try
        {
            users = await partnerUsers.ListUsersAsync(cancellationToken);
        }
        catch (PartnerUserException exception)
        {
            logger.LogError(exception, "Listing the users failed");
            return exception.ToError();
        }

        var pending = await dbContext.PendingRoleAssignments.AsNoTracking().ToListAsync(cancellationToken);

        // A user found in both was just assigned: their row is about to be deleted.
        var activeIds = users.Select(user => Guid.TryParse(user.Id, out var id) ? id : Guid.Empty).ToHashSet();

        return users.Select(user => user.MapToResponse())
            .Concat(pending.Where(assignment => !activeIds.Contains(assignment.UserId)).Select(assignment => assignment.MapToResponse()))
            .OrderBy(user => user.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
