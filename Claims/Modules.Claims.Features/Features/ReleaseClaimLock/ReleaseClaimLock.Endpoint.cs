using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Routes;
using Modules.Common.Features;

namespace Modules.Claims.Features.Features.ReleaseClaimLock;

public sealed class ReleaseClaimLockEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(RouteConsts.ClaimUnlock, Handle)
            .WithName(RouteConsts.ReleaseClaimLockRouteName);
    }

    private static async Task<IResult> Handle(
        Guid claimId,
        [FromBody] ReleaseClaimLockRequest request,
        IValidator<GetClaimByIdRequest> claimIdValidator,
        IValidator<ReleaseClaimLockRequest> requestValidator,
        IReleaseClaimLockHandler handler,
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

        return Results.NoContent();
    }
}
