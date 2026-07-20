using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Modules.Common.Features;

namespace Modules.Common.Features.Tests.Features;

[TestClass]
public sealed class EndpointResultsExtensionsTests
{
    [TestMethod]
    public void ToProblem_WithValidationError_ReturnsValidationProblem()
    {
        var errors = new List<Error> { Error.Validation("Claim.CannotBeNull", "Claim doesn't exist.") };

        var result = errors.ToProblem();

        Assert.IsInstanceOfType<ProblemHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.StatusCode);
        Assert.IsInstanceOfType<HttpValidationProblemDetails>(problem.ProblemDetails, out var validationProblemDetails);
        Assert.IsTrue(validationProblemDetails.Errors.ContainsKey("Claim.CannotBeNull"));
        CollectionAssert.AreEqual(new[] { "Claim doesn't exist." }, validationProblemDetails.Errors["Claim.CannotBeNull"]);
    }

    [TestMethod]
    public void ToProblem_WithNotFoundError_Returns404()
    {
        var errors = new List<Error> { Error.NotFound("Claim.NotFound", "Claim not found.") };

        var result = errors.ToProblem();

        Assert.IsInstanceOfType<ProblemHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status404NotFound, problem.StatusCode);
    }

    [TestMethod]
    public void ToProblem_WithConflictError_Returns409()
    {
        var errors = new List<Error> { Error.Conflict("Claim.Conflict", "Claim already exists.") };

        var result = errors.ToProblem();

        Assert.IsInstanceOfType<ProblemHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status409Conflict, problem.StatusCode);
    }

    [TestMethod]
    public void ToProblem_WithUnauthorizedError_Returns401()
    {
        var errors = new List<Error> { Error.Unauthorized("Claim.Unauthorized", "Not allowed.") };

        var result = errors.ToProblem();

        Assert.IsInstanceOfType<ProblemHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, problem.StatusCode);
    }

    [TestMethod]
    public void ToProblem_WithUnspecifiedErrorType_Returns500()
    {
        var errors = new List<Error> { Error.Failure("Claim.Unexpected", "Something went wrong.") };

        var result = errors.ToProblem();

        Assert.IsInstanceOfType<ProblemHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }

    [TestMethod]
    public void ToProblem_WithEmptyErrorList_Returns500()
    {
        var errors = new List<Error>();

        var result = errors.ToProblem();

        Assert.IsInstanceOfType<ProblemHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }
}
