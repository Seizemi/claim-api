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
            .OrderByDescending(c => c.ClaimDate.DateOfReceivedClaim)
            .ThenBy(c => c.Id);

        var totalCount = await claimsQuery.CountAsync(cancellationToken);

        var items = await claimsQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ClaimMappingExtensions.ToSummaryResponse)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResponse(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages);
    }
}
