using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Mapping;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.GetAllClaims;

internal interface IGetAllClaimsHandler : IHandler
{
    Task<ErrorOr<List<ClaimResponse>>> HandleAsync(CancellationToken cancellationToken);
}

internal sealed class GetAllClaimsHandler(ClaimsDbContext context) : IGetAllClaimsHandler
{
    public async Task<ErrorOr<List<ClaimResponse>>> HandleAsync(CancellationToken cancellationToken)
    {
        var claims = await context.Claims
            .Include(c => c.Booking)
                .ThenInclude(b => b.Customer)
            .Include(c => c.Booking)
                .ThenInclude(b => b.Supplier)
            .Include(c => c.ClaimDate)
            .Include(c => c.Compensation)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return claims.Select(c => c.MapToResponse()).ToList();
    }
}
