using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Modules.Common.Features.Authorization;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Enums;
using Modules.Users.Features.Abstractions;
using Modules.Users.Features.Features.Shared.Errors;
using Modules.Users.Features.Features.Shared.Logging;
using Modules.Users.Features.Features.Shared.Mapping;
using Modules.Users.Features.Features.Shared.Requests;
using Modules.Users.Features.Features.Shared.Responses;
using Modules.Users.Features.RoleAssignment;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Features.CreateUser;

internal interface ICreateUserHandler : IHandler
{
    Task<ErrorOr<UserResponse>> HandleAsync(CreateUserRequest request, CurrentUser currentUser, CancellationToken cancellationToken);
}

/// <summary>
/// Creates the user in Entra without a role and records the role to assign: the role assignment worker assigns it
/// and sends the welcome email (spec 5.8).
/// </summary>
internal sealed partial class CreateUserHandler(
    IPartnerUserService partnerUsers,
    UsersDbContext dbContext,
    RoleAssignmentSignal roleAssignmentSignal,
    TimeProvider timeProvider,
    ILogger<CreateUserHandler> logger) : ICreateUserHandler
{
    public async Task<ErrorOr<UserResponse>> HandleAsync(CreateUserRequest request, CurrentUser currentUser, CancellationToken cancellationToken)
    {
        PartnerUser user;
        try
        {
            user = await partnerUsers.CreateUserAsync(
                new CreatePartnerUserRequest(request.Email, request.DisplayName.Trim(), request.Role),
                cancellationToken);
        }
        catch (PartnerUserException exception) when (exception.Code == PartnerUserErrorCode.GraphError)
        {
            logger.AuditFailed(exception, currentUser.Id, UserManagementAuditLog.Create, targetUserId: null);
            return exception.ToError();
        }
        catch (PartnerUserException exception)
        {
            logger.AuditRejected(currentUser.Id, UserManagementAuditLog.Create, targetUserId: null, exception.Code.ToString());
            return exception.ToError();
        }

        var now = timeProvider.GetUtcNow();
        var assignment = new PendingRoleAssignment
        {
            UserId = Guid.Parse(user.Id),
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = request.Role,
            CreatedBy = currentUser.Id,
            CreatedAt = now,
            Status = RoleAssignmentStatus.Pending,
            NextAttemptAt = now,
            Version = Guid.NewGuid()
        };

        try
        {
            dbContext.PendingRoleAssignments.Add(assignment);
            // The user exists by now: the row is saved even if the client disconnected, hence no cancellation token.
            await dbContext.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.AuditFailed(exception, currentUser.Id, UserManagementAuditLog.Create, user.Id);
            await DeleteUnrecordedUserAsync(user.Id);
            return Error.Unexpected(UserErrorCodes.CreationNotRecorded, UserErrorMessages.CreationNotRecorded);
        }

        roleAssignmentSignal.Notify();
        logger.AuditSucceeded(currentUser.Id, UserManagementAuditLog.Create, user.Id);

        return assignment.MapToResponse();
    }

    // Without its row, nobody would ever assign the user's role nor list them.
    private async Task DeleteUnrecordedUserAsync(string userId)
    {
        try
        {
            await partnerUsers.DeleteUserAsync(userId, CancellationToken.None);
        }
        catch (Exception exception)
        {
            LogOrphanUser(exception, userId);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "The user {UserId} was created without a role and could not be deleted. Delete it manually in the Entra admin center.")]
    private partial void LogOrphanUser(Exception exception, string userId);
}
