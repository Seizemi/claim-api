using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Routes;
using Modules.Common.Features;
using Modules.Common.Features.Authorization;

namespace Modules.Claims.Features.Features.AcquireClaimLock;

public sealed class AcquireClaimLockEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(RouteConsts.ClaimLock, Handle)
            .WithName(RouteConsts.AcquireClaimLockRouteName)
            .RequireAuthorization(AuthPolicies.Agent);
    }

    // The lock holder is the signed-in user: the identity comes from the session, never from the request.
    private static async Task<IResult> Handle(
        Guid claimId,
        CurrentUser currentUser,
        IValidator<GetClaimByIdRequest> claimIdValidator,
        IAcquireClaimLockHandler handler,
        CancellationToken cancellationToken)
    {
        var claimIdValidation = await claimIdValidator.ValidateAsync(
            new GetClaimByIdRequest(claimId), cancellationToken);
        if (!claimIdValidation.IsValid)
        {
            return Results.ValidationProblem(claimIdValidation.ToDictionary());
        }

        var response = await handler.HandleAsync(claimId, currentUser, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
