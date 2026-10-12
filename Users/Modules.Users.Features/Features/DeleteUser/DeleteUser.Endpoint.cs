using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Common.Features;
using Modules.Common.Features.Authorization;
using Modules.Users.Features.Features.Shared.Routes;

namespace Modules.Users.Features.Features.DeleteUser;

internal sealed class DeleteUserEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(RouteConsts.UserById, Handle)
            .RequireAuthorization(AuthPolicies.Supervisor)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> Handle(
        Guid userId,
        CurrentUser currentUser,
        IDeleteUserHandler handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(userId, currentUser, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.NoContent();
    }
}
