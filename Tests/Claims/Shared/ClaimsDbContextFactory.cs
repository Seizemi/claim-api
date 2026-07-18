using Microsoft.EntityFrameworkCore;
using Modules.Claims.Infrastructure.Database;

namespace Modules.Claims.Features.Tests.Shared;

internal static class ClaimsDbContextFactory
{
    internal static ClaimsDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ClaimsDbContext(options);
    }
}
