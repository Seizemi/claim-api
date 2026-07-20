using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.Features;
using Xunit;

namespace Modules.Common.Features.Tests.Features;

public sealed class EndpointResultsExtensionsTests
{
    [Fact]
    public void ToProblem_WithValidationError_ReturnsValidationProblem()
    {
        var errors = new List<Error> { Error.Validation("Claim.CannotBeNull", "Claim doesn't exist.") };

        var result = errors.ToProblem();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        var validationProblemDetails = Assert.IsType<HttpValidationProblemDetails>(problem.ProblemDetails);
        Assert.True(validationProblemDetails.Errors.ContainsKey("Claim.CannotBeNull"));
        Assert.Equal(new[] { "Claim doesn't exist." }, validationProblemDetails.Errors["Claim.CannotBeNull"]);
    }

    [Fact]
    public void ToProblem_WithNotFoundError_Returns404()
    {
        var errors = new List<Error> { Error.NotFound("Claim.NotFound", "Claim not found.") };

        var result = errors.ToProblem();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
    }

    [Fact]
    public void ToProblem_WithConflictError_Returns409()
    {
        var errors = new List<Error> { Error.Conflict("Claim.Conflict", "Claim already exists.") };

        var result = errors.ToProblem();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
    }

    [Fact]
    public void ToProblem_WithUnauthorizedError_Returns401()
    {
        var errors = new List<Error> { Error.Unauthorized("Claim.Unauthorized", "Not allowed.") };

        var result = errors.ToProblem();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.StatusCode);
    }

    [Fact]
    public void ToProblem_WithUnspecifiedErrorType_Returns500()
    {
        var errors = new List<Error> { Error.Failure("Claim.Unexpected", "Something went wrong.") };

        var result = errors.ToProblem();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }

    [Fact]
    public void ToProblem_WithEmptyErrorList_Returns500()
    {
        var errors = new List<Error>();

        var result = errors.ToProblem();

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }
}
