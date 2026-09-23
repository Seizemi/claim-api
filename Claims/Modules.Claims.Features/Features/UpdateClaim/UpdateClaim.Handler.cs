using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Validators;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.UpdateClaim;

internal interface IUpdateClaimHandler : IHandler
{
    Task<ErrorOr<Updated>> HandleAsync(Guid claimId, ClaimRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateClaimHandler(ClaimsDbContext context, TimeProvider timeProvider) : IUpdateClaimHandler
{
    public async Task<ErrorOr<Updated>> HandleAsync(Guid claimId, ClaimRequest request, CancellationToken cancellationToken)
    {
        var claim = await context.Claims
            .Include(c => c.Booking)
                .ThenInclude(b => b.Customer)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .FirstOrDefaultAsync(c => c.Id == claimId, cancellationToken);

        if (claim is null)
        {
            return Error.Validation(
                ClaimErrorCodes.ClaimCannotBeNull,
                ClaimErrorMessages.ClaimCannotBeNull);
        }

        var staleThreshold = timeProvider.GetUtcNow().AddMinutes(-15);
        var isLockedByAnotherUser = claim.LockedByUserId is not null
            && claim.LockedByUserId != request.EditingUserId
            && claim.LockedAt > staleThreshold;

        if (isLockedByAnotherUser)
        {
            return Error.Conflict(
                ClaimErrorCodes.ClaimLockedByAnotherUser,
                $"Claim is currently locked by {claim.LockedByUserName}.");
        }

        var lookupErrors = await context.ValidateLookupsExistAsync(request, cancellationToken);
        if (lookupErrors.Count > 0)
        {
            return lookupErrors;
        }

        claim.UpdateFrom(request);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
