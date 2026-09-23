using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.ReleaseClaimLock;

internal interface IReleaseClaimLockHandler : IHandler
{
    Task<ErrorOr<Success>> HandleAsync(Guid claimId, ReleaseClaimLockRequest request, CancellationToken cancellationToken);
}

internal sealed class ReleaseClaimLockHandler(ClaimsDbContext context) : IReleaseClaimLockHandler
{
    public async Task<ErrorOr<Success>> HandleAsync(Guid claimId, ReleaseClaimLockRequest request, CancellationToken cancellationToken)
    {
        var rowsAffected = await context.Claims
            .Where(c => c.Id == claimId && c.LockedByUserId == request.UserId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.LockedByUserId, (Guid?)null)
                .SetProperty(c => c.LockedByUserName, (string?)null)
                .SetProperty(c => c.LockedAt, (DateTimeOffset?)null),
                cancellationToken);

        if (rowsAffected == 1)
        {
            return Result.Success;
        }

        var exists = await context.Claims.AnyAsync(c => c.Id == claimId, cancellationToken);
        if (!exists)
        {
            return Error.Validation(ClaimErrorCodes.ClaimCannotBeNull, ClaimErrorMessages.ClaimCannotBeNull);
        }

        return Result.Success;
    }
}
