---
name: create-feature
description: Add a new feature (use case) to a module following our Vertical Slice Architecture. Use whenever the user asks to add a use case, command, or query to a module.
---

# Add a Feature

Create one folder per use case: `Features/<UseCase>/`. Inside it:

1. `<UseCase>.Endpoint.cs` - implements `IEndpointModule`, maps the route from `RouteConsts`, validates the request, and calls the handler.
2. `<UseCase>.Handler.cs` - internal interface + internal sealed implementation, returns `ErrorOr<T>`.
3. `<UseCase>.Validator.cs` - FluentValidation validator for the request. Use error codes from `Shared/Errors/ClaimErrors.cs` and messages from `ClaimErrorMessages`.

Follow these reference files exactly:
- `Features/GetClaimById/GetClaimById.Endpoint.cs`
- `Features/GetClaimById/GetClaimById.Handler.cs`
- `Features/GetClaimById/GetClaimById.Validator.cs`

## Conventions

- Request records go in `Features/Shared/Requests/`.
- Mapping extension methods go in `Features/Shared/Mapping/ClaimMappingExtensions.cs` if shared across use cases.
- Error codes and messages come from `Features/Shared/Errors/ClaimErrors.cs`.
- Routes are constants in `Features/Shared/Routes/RouteConsts.cs`.
- Handler interface and implementation are both `internal`. The implementing method is `public` only to satisfy the interface contract.
- Perform FluentValidation in the endpoint before calling the handler.
- Handlers return `ErrorOr<T>`. Never use exceptions for business errors.
- Endpoints return `IResult` using `Results.Ok()`, `Results.ValidationProblem()`, or `response.Errors.ToProblem()`.
- Use `AsNoTracking()` on all read-only queries.
- Use primary constructors for dependency injection.
- Use file-scoped namespaces.
