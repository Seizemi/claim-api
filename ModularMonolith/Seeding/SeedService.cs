using Microsoft.EntityFrameworkCore;
using Modules.Claims.Infrastructure.Database;

namespace ModularMonolith.Seeding;

public sealed class SeedService(ClaimsDbContext context, ILogger<SeedService> logger)
{
    private const int ClaimCount = 1000;

    public async Task SeedDataAsync(CancellationToken cancellationToken = default)
    {
        if (await context.Claims.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Data already exists, skipping seeding");
            return;
        }

        logger.LogInformation("Starting data seeding...");

        var random = new Random();

        var lookups = SeedDataGenerator.GenerateLookups();
        await context.Reasons.AddRangeAsync(lookups.Reasons, cancellationToken);
        await context.Solutions.AddRangeAsync(lookups.Solutions, cancellationToken);
        await context.FollowedBies.AddRangeAsync(lookups.FollowedBies, cancellationToken);
        await context.RefundStates.AddRangeAsync(lookups.RefundStates, cancellationToken);
        await context.CompensationReasons.AddRangeAsync(lookups.CompensationReasons, cancellationToken);
        await context.SalesChannels.AddRangeAsync(lookups.SalesChannels, cancellationToken);
        await context.Services.AddRangeAsync(lookups.Services, cancellationToken);
        await context.SkissimTypes.AddRangeAsync(lookups.SkissimTypes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var suppliers = SeedDataGenerator.GenerateSuppliers(lookups.Services, random);
        await context.Suppliers.AddRangeAsync(suppliers, cancellationToken);

        var claims = Enumerable.Range(0, ClaimCount)
            .Select(_ => SeedDataGenerator.GenerateClaim(suppliers[random.Next(suppliers.Count)], lookups, random))
            .ToList();
        await context.Claims.AddRangeAsync(claims, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Data seeding completed: {ClaimCount} claims across {SupplierCount} suppliers", ClaimCount, suppliers.Count);
    }
}
