using Microsoft.EntityFrameworkCore;
using Modules.Claims.Infrastructure.Database;

namespace ModularMonolith.Seeding;

public sealed class SeedService(ClaimsDbContext context, ILogger<SeedService> logger)
{
    private const int ClaimCount = 1000;
    private const int SupplierPoolSize = 100;

    public async Task SeedDataAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Claims.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Data already exists, skipping seeding");
            return;
        }

        logger.LogInformation("Starting data seeding...");

        var random = new Random();
        var suppliers = SeedDataGenerator.GenerateSuppliers(SupplierPoolSize, random);
        await context.Suppliers.AddRangeAsync(suppliers, cancellationToken);

        var claims = Enumerable.Range(0, ClaimCount)
            .Select(_ => SeedDataGenerator.GenerateClaim(suppliers[random.Next(suppliers.Count)], random))
            .ToList();
        await context.Claims.AddRangeAsync(claims, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Data seeding completed: {ClaimCount} claims across {SupplierCount} suppliers", ClaimCount, suppliers.Count);
    }
}
