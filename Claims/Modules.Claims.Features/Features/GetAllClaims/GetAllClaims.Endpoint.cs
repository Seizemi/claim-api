using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Routes;
using Modules.Common.Features;

namespace Modules.Claims.Features.Features.GetAllClaims;

public sealed class GetAllClaimsEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(RouteConsts.DashboardClaim, Handle);
    }

    private static async Task<IResult> Handle(
        [AsParameters] GetAllClaimsRequest request,
        IValidator<GetAllClaimsRequest> validator,
        IGetAllClaimsHandler handler,
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
