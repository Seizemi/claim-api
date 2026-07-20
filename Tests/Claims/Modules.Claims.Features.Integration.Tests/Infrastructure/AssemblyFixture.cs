namespace Modules.Claims.Features.Integration.Tests.Infrastructure;

[TestClass]
public static class AssemblyFixture
{
    internal static IntegrationTestWebAppFactory Factory { get; private set; } = null!;

    [AssemblyInitialize]
    public static async Task AssemblyInitializeAsync(TestContext testContext)
    {
        Factory = new IntegrationTestWebAppFactory();
        await Factory.InitializeAsync();

        // Forces the WebApplicationFactory to build its host now (running Program.cs's
        // startup migration once), rather than lazily on the first test's request.
        _ = Factory.Server;
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanupAsync() => await Factory.DisposeAsync();
}
