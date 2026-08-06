using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Domain;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.GetClaimsBySeason;

internal interface IGetClaimsBySeasonHandler : IHandler
{
    Task<ErrorOr<IReadOnlyList<ClaimResponse>>> HandleAsync(GetClaimsBySeasonRequest request, CancellationToken cancellationToken);
}

internal sealed class GetClaimsBySeasonHandler(ClaimsDbContext context) : IGetClaimsBySeasonHandler
{
    public async Task<ErrorOr<IReadOnlyList<ClaimResponse>>> HandleAsync(GetClaimsBySeasonRequest request, CancellationToken cancellationToken)
    {
        SeasonCalculator.TryResolveDateRange(request.SeasonValue, out var startDate, out var endDate);

        var claims = await context.Claims
            .Where(c => c.ClaimDate.DateOfArrival >= startDate && c.ClaimDate.DateOfArrival <= endDate)
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
            .OrderByDescending(c => c.ClaimDate.DateOfReceivedClaim)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

        return claims.Select(ClaimMappingExtensions.MapToResponse).ToList();
    }
}
