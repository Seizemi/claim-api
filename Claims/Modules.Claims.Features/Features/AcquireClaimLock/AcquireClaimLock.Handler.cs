using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.AcquireClaimLock;

internal interface IAcquireClaimLockHandler : IHandler
{
    Task<ErrorOr<AcquireClaimLockResponse>> HandleAsync(Guid claimId, AcquireClaimLockRequest request, CancellationToken cancellationToken);
}

internal sealed class AcquireClaimLockHandler(ClaimsDbContext context, TimeProvider timeProvider) : IAcquireClaimLockHandler
{
    private static readonly TimeSpan LockTtl = TimeSpan.FromMinutes(15);

    public async Task<ErrorOr<AcquireClaimLockResponse>> HandleAsync(
        Guid claimId, AcquireClaimLockRequest request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var staleThreshold = now - LockTtl;

        var rowsAffected = await context.Claims
            .Where(c => c.Id == claimId
                && (c.LockedByUserId == null || c.LockedAt < staleThreshold))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.LockedByUserId, request.UserId)
                .SetProperty(c => c.LockedByUserName, request.UserName)
                .SetProperty(c => c.LockedAt, now),
                cancellationToken);

        if (rowsAffected == 1)
        {
            var claim = await context.Claims
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Customer)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.Supplier)
                        .ThenInclude(s => s.Service)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.SalesChannel)
                .Include(c => c.Booking)
                    .ThenInclude(b => b.SkissimType)
                .Include(c => c.ClaimDate)
                .Include(c => c.Compensation)
                    .ThenInclude(comp => comp.RefundState)
                .Include(c => c.Compensation)
                    .ThenInclude(comp => comp.CompensationReason)
                .Include(c => c.Reason)
                .Include(c => c.Solution)
                .Include(c => c.FollowedBy)
                .AsNoTracking()
                .FirstAsync(c => c.Id == claimId, cancellationToken);

            return new AcquireClaimLockResponse(true, request.UserId, request.UserName, now, claim.MapToResponse(timeProvider));
        }

        var current = await context.Claims
            .AsNoTracking()
            .Where(c => c.Id == claimId)
            .Select(c => new { c.LockedByUserId, c.LockedByUserName, c.LockedAt })
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            return Error.Validation(ClaimErrorCodes.ClaimCannotBeNull, ClaimErrorMessages.ClaimCannotBeNull);
        }

        return new AcquireClaimLockResponse(false, current.LockedByUserId, current.LockedByUserName, current.LockedAt, Claim: null);
    }
}
