namespace Modules.Users.Features.Features.Shared.Routes;

internal static class RouteConsts
{
    internal const string ApiPrefix = "/api";
    internal const string CurrentUser = $"{ApiPrefix}/me";
    internal const string Users = $"{ApiPrefix}/users";
    internal const string UserById = $"{Users}/{{userId:guid}}";
}
