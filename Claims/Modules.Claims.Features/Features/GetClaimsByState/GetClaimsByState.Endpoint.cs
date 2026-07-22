using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Routes;
using Modules.Common.Features;

namespace Modules.Claims.Features.Features.GetClaimsByState;

public sealed class GetClaimsByStateEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(RouteConsts.ClaimsByState, Handle);
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetClaimsByStateRequest request,
        IValidator<GetClaimsByStateRequest> validator,
        IGetClaimsByStateHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(request, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
