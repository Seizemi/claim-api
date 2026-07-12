using ErrorOr;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.CreateClaim;

internal interface ICreateClaimHandler : IHandler
{
    Task<ErrorOr<Guid>> HandleAsync(ClaimRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateClaimHandler(ClaimsDbContext context) : ICreateClaimHandler
{
    public async Task<ErrorOr<Guid>> HandleAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        var claimId = Guid.CreateVersion7();
        var claim = request.MapToClaim(claimId);

        await context.Claims.AddAsync(claim, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return claimId;
    }
}
