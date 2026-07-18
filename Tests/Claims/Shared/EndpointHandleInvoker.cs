using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Modules.Claims.Features.Tests.Shared;

internal static class EndpointHandleInvoker
{
    internal static async Task<IResult> InvokeAsync(Type endpointType, params object?[] arguments)
    {
        var method = endpointType.GetMethod("Handle", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"No private static 'Handle' method found on {endpointType.Name}.");

        var task = (Task<IResult>)method.Invoke(null, arguments)!;
        return await task;
    }
}
