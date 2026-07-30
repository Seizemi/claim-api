using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Modules.Claims.Domain.Entities;
using Modules.Claims.Features.Integration.Tests.Shared;
using Modules.Claims.Infrastructure.Database;
using Testcontainers.PostgreSql;
using Xunit;

namespace Modules.Claims.Features.Integration.Tests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("claimapi_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    internal string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        // Forces the WebApplicationFactory to build its host now (running Program.cs's
        // startup migration once), rather than lazily on the first test's request.
        _ = Server;

        await SeedLookupsAsync();
    }

    private async Task SeedLookupsAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

        context.Reasons.Add(new Reason { Id = LookupTestIds.ReasonId, Label = "Test reason", Value = "TestReason" });
        context.Solutions.Add(new Solution { Id = LookupTestIds.SolutionId, Label = "Test solution", Value = "TestSolution" });
        context.FollowedBies.Add(new FollowedBy { Id = LookupTestIds.FollowedById, Label = "Test follower", Value = "TestFollower" });
        context.RefundStates.Add(new RefundState { Id = LookupTestIds.RefundStateId, Label = "Test refund state", Value = "TestRefundState" });
        context.CompensationReasons.Add(new CompensationReason { Id = LookupTestIds.CompensationReasonId, Label = "Test compensation reason", Value = "TestCompensationReason" });
        context.SalesChannels.Add(new SalesChannel { Id = LookupTestIds.SalesChannelId, Label = "Test channel", Value = "TestChannel", Language = "Fr" });
        context.Services.Add(new Service { Id = LookupTestIds.ServiceId, Label = "Test service", Value = "TestService" });
        context.SkissimTypes.Add(new SkissimType { Id = LookupTestIds.SkissimTypeId, Label = "Test skissim type", Value = "TestSkissimType" });

        await context.SaveChangesAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", ConnectionString);
        builder.UseSetting("Seeding:Enabled", "false");
    }
}
