using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.Common.Features;

namespace Modules.Common.Features.Tests.Features;

[TestClass]
public sealed class GlobalExceptionHandlerTests
{
    private static readonly GlobalExceptionHandler Handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [TestMethod]
    public async Task TryHandleAsync_WithBadHttpRequestException_ReturnsExceptionStatusCodeWithBadRequestTitle()
    {
        var httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        var exception = new BadHttpRequestException("Malformed request body.", StatusCodes.Status400BadRequest);

        var handled = await Handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.IsTrue(handled);
        Assert.AreEqual(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);

        var problem = await ReadProblemDetailsAsync(httpContext);
        Assert.AreEqual("Bad request", problem.Title);
        Assert.AreEqual("Malformed request body.", problem.Detail);
        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.Status);
    }

    [TestMethod]
    public async Task TryHandleAsync_WithGenericException_Returns500WithUnexpectedErrorTitle()
    {
        var httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        var exception = new InvalidOperationException("Database connection failed.");

        var handled = await Handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.IsTrue(handled);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);

        var problem = await ReadProblemDetailsAsync(httpContext);
        Assert.AreEqual("An unexpected error occurred", problem.Title);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, problem.Status);
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(DefaultHttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(httpContext.Response.Body);
        Assert.IsNotNull(problem);
        return problem;
    }
}
