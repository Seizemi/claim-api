using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.GetClaimById;

internal interface IGetClaimByIdHandler : IHandler
{
    Task<ErrorOr<ClaimResponse>> HandleAsync(Guid claimId, CancellationToken cancellationToken);
}

internal sealed class GetClaimByIdHandler(ClaimsDbContext context) : IGetClaimByIdHandler
{
    public async Task<ErrorOr<ClaimResponse>> HandleAsync(Guid claimId, CancellationToken cancellationToken)
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
            .FirstOrDefaultAsync(c => c.Id == claimId, cancellationToken);

        if (claim is null)
        {
            return Error.Validation(
                ClaimErrorCodes.ClaimCannotBeNull,
                ClaimErrorMessages.ClaimCannotBeNull);
        }

        return claim.MapToResponse();
    }
}
