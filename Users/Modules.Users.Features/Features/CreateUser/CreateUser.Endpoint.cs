using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Modules.Common.Features;
using Modules.Common.Features.Authorization;
using Modules.Users.Features.Features.Shared.Requests;
using Modules.Users.Features.Features.Shared.Responses;
using Modules.Users.Features.Features.Shared.Routes;

namespace Modules.Users.Features.Features.CreateUser;

internal sealed class CreateUserEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(RouteConsts.Users, Handle)
            .RequireAuthorization(AuthPolicies.Supervisor)
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status502BadGateway);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateUserRequest request,
        CurrentUser currentUser,
        IValidator<CreateUserRequest> validator,
        ICreateUserHandler handler,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var response = await handler.HandleAsync(request, currentUser, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Created($"{RouteConsts.Users}/{response.Value.Id}", response.Value);
    }
}
