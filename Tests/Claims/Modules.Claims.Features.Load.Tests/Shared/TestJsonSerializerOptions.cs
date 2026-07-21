using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modules.Claims.Features.Load.Tests.Shared;

internal static class TestJsonSerializerOptions
{
    // Mirrors ModularMonolith/Program.cs's ConfigureHttpJsonOptions, which serializes enums as
    // strings. HttpClient's default request options expect numeric enums, so requests
    // containing enum fields (ClaimRequest.State, etc.) fail to serialize without this.
    internal static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
