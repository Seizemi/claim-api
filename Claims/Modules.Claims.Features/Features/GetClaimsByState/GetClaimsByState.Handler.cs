using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.GetClaimsByState;

internal interface IGetClaimsByStateHandler : IHandler
{
    Task<ErrorOr<PagedResponse>> HandleAsync(GetClaimsByStateRequest request, CancellationToken cancellationToken);
}

internal sealed class GetClaimsByStateHandler(ClaimsDbContext context) : IGetClaimsByStateHandler
{
    internal const int MaxPageSize = 100;

    public async Task<ErrorOr<PagedResponse>> HandleAsync(GetClaimsByStateRequest request, CancellationToken cancellationToken)
    {
        var claimsQuery = context.Claims
            .Where(c => c.State == request.ClaimState)
            .Include(c => c.Booking)
                .ThenInclude(b => b.Customer)
            .Include(c => c.Booking)
                .ThenInclude(b => b.Supplier)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .AsNoTracking()
            .OrderByDescending(c => c.ClaimDate.DateOfReceivedClaim);

        var totalCount = await claimsQuery.CountAsync(cancellationToken);

        var claims = await claimsQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResponse(
            claims.Select(c => c.MapToResponse()).ToList(),
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages);
    }
}
