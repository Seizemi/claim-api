using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Common.Features.Authorization;
using Modules.Users.Domain.Entities;
using Modules.Users.Features.Abstractions;
using Modules.Users.Features.Features.Shared.Errors;
using Modules.Users.Features.Features.Shared.Logging;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Features.DeleteUser;

internal interface IDeleteUserHandler : IHandler
{
    Task<ErrorOr<Deleted>> HandleAsync(Guid userId, CurrentUser currentUser, CancellationToken cancellationToken);
}

internal sealed class DeleteUserHandler(
    IPartnerUserService partnerUsers,
    UsersDbContext dbContext,
    ILogger<DeleteUserHandler> logger) : IDeleteUserHandler
{
    public async Task<ErrorOr<Deleted>> HandleAsync(Guid userId, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        var targetUserId = userId.ToString();

        if (userId == currentUser.Id)
        {
            return Reject(Error.Validation(UserErrorCodes.CannotDeleteSelf, UserErrorMessages.CannotDeleteSelf));
        }

        try
        {
            var pending = await dbContext.PendingRoleAssignments.FindAsync([userId], cancellationToken);
            if (pending is not null)
            {
                await DeletePendingUserAsync(pending, cancellationToken);
            }
            else
            {
                // Only users of this application can be deleted: a tenant user without a role here is "not found".
                var user = await partnerUsers.GetUserAsync(targetUserId, cancellationToken);
                if (user?.Role is null)
                {
                    return Reject(Error.NotFound(UserErrorCodes.NotFound, UserErrorMessages.NotFound));
                }

                if (user.Role == AppRoles.Supervisor)
                {
                    return Reject(Error.Validation(UserErrorCodes.CannotDeleteSupervisor, UserErrorMessages.CannotDeleteSupervisor));
                }

                await partnerUsers.DeleteUserAsync(targetUserId, cancellationToken);
            }
        }
        catch (PartnerUserException exception) when (exception.Code == PartnerUserErrorCode.GraphError)
        {
            logger.AuditFailed(exception, currentUser.Id, UserManagementAuditLog.Delete, targetUserId);
            return exception.ToError();
        }
        catch (PartnerUserException exception)
        {
            return Reject(exception.ToError());
        }

        logger.AuditSucceeded(currentUser.Id, UserManagementAuditLog.Delete, targetUserId);
        return Result.Deleted;

        Error Reject(Error error)
        {
            logger.AuditRejected(currentUser.Id, UserManagementAuditLog.Delete, targetUserId, error.Code);
            return error;
        }
    }

    /// <summary>
    /// A user created from the application whose role is not (or could not be) assigned. Their role is always an
    /// assignable one, so the Supervisor rule doesn't apply.
    /// </summary>
    private async Task DeletePendingUserAsync(PendingRoleAssignment pending, CancellationToken cancellationToken)
    {
        try
        {
            await partnerUsers.DeleteUserAsync(pending.UserId.ToString(), cancellationToken);
        }
        catch (PartnerUserException exception) when (exception.Code == PartnerUserErrorCode.UserNotFound)
        {
            // Already deleted in Entra (e.g. from the admin center): only the row is left.
        }

        dbContext.PendingRoleAssignments.Remove(pending);
        try
        {
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The worker assigned the role and deleted the row meanwhile: the user is deleted all the same.
        }
    }
}
