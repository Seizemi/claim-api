using System.Text.Json;
using System.Text.Json.Serialization;

namespace Modules.Claims.Features.Integration.Tests.Shared;

internal static class TestJsonSerializerOptions
{
    // Mirrors ModularMonolith/Program.cs's ConfigureHttpJsonOptions, which serializes enums as
    // strings. HttpClient's default ReadFromJsonAsync options expect numeric enums, so responses
    // containing enum fields (ClaimResponse.State, etc.) fail to deserialize without this.
    internal static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
