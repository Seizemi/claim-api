using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Modules.Common.Features;

public static class EndpointResultsExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Results.Problem();
        }

        return CreateProblem(errors);
    }

    private static IResult CreateProblem(List<Error> errors)
    {
        var statusCode = errors.First().Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            // Error.Custom(type: <HTTP status code>, ...) for statuses ErrorType has no member for, e.g. 502.
            var type when (int)type is >= 400 and <= 599 => (int)type,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.ValidationProblem(errors.ToDictionary(k => k.Code, v => new[] { v.Description }),
            statusCode: statusCode);
    }
}
