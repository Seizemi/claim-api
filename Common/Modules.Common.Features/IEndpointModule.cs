using Microsoft.AspNetCore.Routing;

namespace Modules.Common.Features;

public interface IEndpointModule
{
    void AddRoutes(IEndpointRouteBuilder app);
}
