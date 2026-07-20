using Microsoft.Extensions.DependencyInjection;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Integration.Tests.Infrastructure;

[TestClass]
public abstract class IntegrationTestBase
{
    private static DatabaseResetHelper _resetHelper = null!;
    protected static HttpClient Client { get; private set; } = null!;

    private IServiceScope? _scope;
    protected ClaimsDbContext DbContext =>
        (_scope ??= AssemblyFixture.Factory.Services.CreateScope())
            .ServiceProvider.GetRequiredService<ClaimsDbContext>();

    [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
    public static void ClassInitialize(TestContext _)
    {
        Client = AssemblyFixture.Factory.CreateClient();
        _resetHelper = new DatabaseResetHelper(AssemblyFixture.Factory.Services);
    }

    [TestCleanup]
    public async Task TestCleanupAsync()
    {
        _scope?.Dispose();
        _scope = null;
        await _resetHelper.ResetAsync();
    }
}
