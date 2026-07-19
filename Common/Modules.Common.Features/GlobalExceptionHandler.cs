using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Modules.Common.Features;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException badHttpRequestException)
        {
            logger.LogWarning(exception, "Bad request received");

            httpContext.Response.StatusCode = badHttpRequestException.StatusCode;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = badHttpRequestException.StatusCode,
                Title = "Bad request",
                Detail = badHttpRequestException.Message
            }, cancellationToken);

            return true;
        }

        logger.LogError(exception, "Unhandled exception occurred");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred",
            Detail = "An unexpected error occurred. Please try again later."
        }, cancellationToken);

        return true;
    }
}
