using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Database;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<PendingRoleAssignment> PendingRoleAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DbConsts.UsersSchemaName);

        modelBuilder.Entity<PendingRoleAssignment>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.UserId).ValueGeneratedNever();
            entity.Property(x => x.Email).IsRequired().HasMaxLength(254);
            entity.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Role).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.LastError).HasMaxLength(500);
            entity.Property(x => x.Version).IsConcurrencyToken();

            // The worker's query: due rows of a status.
            entity.HasIndex(x => new { x.Status, x.NextAttemptAt });
        });
    }
}
