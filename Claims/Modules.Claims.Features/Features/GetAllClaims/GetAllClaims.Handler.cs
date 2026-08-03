using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.GetAllClaims;

internal interface IGetAllClaimsHandler : IHandler
{
    Task<ErrorOr<PagedResponse>> HandleAsync(GetAllClaimsRequest request, CancellationToken cancellationToken);
}

internal sealed class GetAllClaimsHandler(ClaimsDbContext context) : IGetAllClaimsHandler
{
    internal const int MaxPageSize = 100;

    public async Task<ErrorOr<PagedResponse>> HandleAsync(GetAllClaimsRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        var claimsQuery = context.Claims
            .OrderByDescending(c => c.ClaimDate.DateOfReceivedClaim)
            .ThenBy(c => c.Id);

        var totalCount = await claimsQuery.CountAsync(cancellationToken);

        var items = await claimsQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ClaimMappingExtensions.ToSummaryResponse)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse(
            items,
            pageNumber,
            pageSize,
            totalCount,
            totalPages);
    }
}
