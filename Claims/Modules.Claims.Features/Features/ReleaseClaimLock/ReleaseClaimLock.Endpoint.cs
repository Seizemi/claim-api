using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Routes;
using Modules.Common.Features;
using Modules.Common.Features.Authorization;

namespace Modules.Claims.Features.Features.ReleaseClaimLock;

public sealed class ReleaseClaimLockEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(RouteConsts.ClaimUnlock, Handle)
            .WithName(RouteConsts.ReleaseClaimLockRouteName)
            .RequireAuthorization(AuthPolicies.Agent);
    }

    // Only the signed-in user's own lock is released: the identity comes from the session, never from the request.
    private static async Task<IResult> Handle(
        Guid claimId,
        CurrentUser currentUser,
        IValidator<GetClaimByIdRequest> claimIdValidator,
        IReleaseClaimLockHandler handler,
        CancellationToken cancellationToken)
    {
        var claimIdValidation = await claimIdValidator.ValidateAsync(
            new GetClaimByIdRequest(claimId), cancellationToken);
        if (!claimIdValidation.IsValid)
        {
            return Results.ValidationProblem(claimIdValidation.ToDictionary());
        }

        var response = await handler.HandleAsync(claimId, currentUser.Id, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.NoContent();
    }
}
