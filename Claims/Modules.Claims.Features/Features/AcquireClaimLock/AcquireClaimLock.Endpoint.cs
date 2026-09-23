using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Routes;
using Modules.Common.Features;

namespace Modules.Claims.Features.Features.AcquireClaimLock;

public sealed class AcquireClaimLockEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(RouteConsts.ClaimLock, Handle)
            .WithName(RouteConsts.AcquireClaimLockRouteName);
    }

    private static async Task<IResult> Handle(
        Guid claimId,
        [FromBody] AcquireClaimLockRequest request,
        IValidator<GetClaimByIdRequest> claimIdValidator,
        IValidator<AcquireClaimLockRequest> requestValidator,
        IAcquireClaimLockHandler handler,
        CancellationToken cancellationToken)
    {
        var claimIdValidation = await claimIdValidator.ValidateAsync(
            new GetClaimByIdRequest(claimId), cancellationToken);
        if (!claimIdValidation.IsValid)
        {
            return Results.ValidationProblem(claimIdValidation.ToDictionary());
        }

        var requestValidation = await requestValidator.ValidateAsync(request, cancellationToken);
        if (!requestValidation.IsValid)
        {
            return Results.ValidationProblem(requestValidation.ToDictionary());
        }

        var response = await handler.HandleAsync(claimId, request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
