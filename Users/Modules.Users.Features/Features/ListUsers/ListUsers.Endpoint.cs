using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Common.Features;
using Modules.Common.Features.Authorization;
using Modules.Users.Features.Features.Shared.Responses;
using Modules.Users.Features.Features.Shared.Routes;

namespace Modules.Users.Features.Features.ListUsers;

internal sealed class ListUsersEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(RouteConsts.Users, Handle)
            .RequireAuthorization(AuthPolicies.Supervisor)
            .Produces<IReadOnlyList<UserResponse>>()
            .ProducesProblem(StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> Handle(IListUsersHandler handler, CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
