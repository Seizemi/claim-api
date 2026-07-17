using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Infrastructure.Database;
using Modules.Common.Features;

namespace Modules.Claims.Features.Features.GetAllClaims;

internal interface IGetAllClaimsHandler : IHandler
{
    Task<ErrorOr<PagedResponse<ClaimResponse>>> HandleAsync(GetAllClaimsRequest request, CancellationToken cancellationToken);
}

internal sealed class GetAllClaimsHandler(ClaimsDbContext context) : IGetAllClaimsHandler
{
    internal const int MaxPageSize = 100;

    public async Task<ErrorOr<PagedResponse<ClaimResponse>>> HandleAsync(GetAllClaimsRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;

        var claimsQuery = context.Claims
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
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<ClaimResponse>(
            claims.Select(c => c.MapToResponse()).ToList(),
            pageNumber,
            pageSize,
            totalCount,
            totalPages);
    }
}
