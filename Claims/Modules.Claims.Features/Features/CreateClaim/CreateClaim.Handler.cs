using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Modules.Claims.Features.Abstractions;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Features.CreateClaim;

internal interface ICreateClaimHandler : IHandler
{
    Task<ErrorOr<Created>> HandleAsync(ClaimRequest request, CancellationToken cancellationToken);
}

internal sealed class CreateClaimHandler(ClaimsDbContext context) : ICreateClaimHandler
{
    public async Task<ErrorOr<Created>> HandleAsync(ClaimRequest request, CancellationToken cancellationToken)
    {
        var claimExists = await context.Claims.AnyAsync(c => c.Id == request.Id, cancellationToken);
        if (claimExists)
        {
            return Error.Conflict(ClaimErrorCodes.ClaimAlreadyExists, ClaimErrorMessages.ClaimAlreadyExists);
        }

        var claim = request.MapToClaim();

        await context.Claims.AddAsync(claim, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Created;
    }
}
