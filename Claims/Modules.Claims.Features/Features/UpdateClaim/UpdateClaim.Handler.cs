using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.UpdateClaim;

internal interface IUpdateClaimHandler : IHandler
{
    Task<ErrorOr<Updated>> HandleAsync(Guid claimId, ClaimRequest request, CancellationToken cancellationToken);
}

internal sealed class UpdateClaimHandler(ClaimsDbContext context) : IUpdateClaimHandler
{
    public async Task<ErrorOr<Updated>> HandleAsync(Guid claimId, ClaimRequest request, CancellationToken cancellationToken)
    {
        var claim = await context.Claims
            .Include(c => c.Booking)
                .ThenInclude(b => b.Customer)
            .Include(c => c.Booking)
                .ThenInclude(b => b.Supplier)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .FirstOrDefaultAsync(c => c.Id == claimId, cancellationToken);

        if (claim is null)
        {
            return Error.Validation(
                ClaimErrorCodes.ClaimCannotBeNull,
                ClaimErrorMessages.ClaimCannotBeNull);
        }

        claim.UpdateFrom(request);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
