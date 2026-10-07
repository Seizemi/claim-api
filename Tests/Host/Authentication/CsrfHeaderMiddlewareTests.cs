using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Authentication;
using Xunit;

namespace ModularMonolith.Tests.Authentication;

public sealed class CsrfHeaderMiddlewareTests
{
    private static readonly IServiceProvider Services = new ServiceCollection()
        .AddLogging()
        .AddProblemDetails()
        .BuildServiceProvider();

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    [InlineData("post")]
    public async Task InvokeAsync_StateChangingApiRequestWithoutHeader_Returns400ProblemDetails(string method)
    {
        var (context, nextCalled) = await InvokeAsync(method, "/api/v1.0/Claim/new-claim/claim", csrfHeaderValue: null);

        Assert.False(nextCalled());
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);

        var problem = await ReadJsonAsync(context);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Contains("X-CSRF", problem.GetProperty("detail").GetString());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("true")]
    [InlineData("")]
    public async Task InvokeAsync_StateChangingApiRequestWithWrongHeaderValue_Returns400(string headerValue)
    {
        var (context, nextCalled) = await InvokeAsync("POST", "/api/me", headerValue);

        Assert.False(nextCalled());
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task InvokeAsync_StateChangingApiRequestWithHeader_CallsNext(string method)
    {
        var (context, nextCalled) = await InvokeAsync(method, "/api/v1.0/Claim/claim-details/1/lock", CsrfHeaderMiddleware.HeaderValue);

        Assert.True(nextCalled());
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public async Task InvokeAsync_SafeApiRequestWithoutHeader_CallsNext(string method)
    {
        var (_, nextCalled) = await InvokeAsync(method, "/api/me", csrfHeaderValue: null);

        Assert.True(nextCalled());
    }

    [Theory]
    [InlineData("/auth/logout")]
    [InlineData("/signin-oidc")]
    [InlineData("/apiary")]
    [InlineData("/")]
    public async Task InvokeAsync_PostOutsideApiWithoutHeader_CallsNext(string path)
    {
        var (_, nextCalled) = await InvokeAsync("POST", path, csrfHeaderValue: null);

        Assert.True(nextCalled());
    }

    [Fact]
    public async Task InvokeAsync_ApiPathWithDifferentCasing_RequiresHeader()
    {
        var (context, nextCalled) = await InvokeAsync("DELETE", "/API/users/1", csrfHeaderValue: null);

        Assert.False(nextCalled());
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    private static async Task<(HttpContext Context, Func<bool> NextCalled)> InvokeAsync(string method, string path, string? csrfHeaderValue)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = Services,
            Request = { Method = method, Path = path },
            Response = { Body = new MemoryStream() }
        };

        if (csrfHeaderValue is not null)
        {
            context.Request.Headers[CsrfHeaderMiddleware.HeaderName] = csrfHeaderValue;
        }

        var nextCalled = false;
        var middleware = new CsrfHeaderMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        return (context, () => nextCalled);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body, cancellationToken: TestContext.Current.CancellationToken);

        return document.RootElement.Clone();
    }
}
