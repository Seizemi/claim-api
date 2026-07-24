using Microsoft.EntityFrameworkCore;
using Modules.Claims.Domain.Entities;

namespace Modules.Claims.Infrastructure.Database;

public class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<ClaimDate> ClaimDates { get; set; }
    public DbSet<Compensation> Compensations { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(DbConsts.ClaimsSchemaName);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.AkioNumber).IsRequired();

            entity.HasMany(x => x.Bookings)
                .WithOne(x => x.Customer)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.SupplierAkioNumber).IsRequired();

            entity.HasMany(x => x.Bookings)
                .WithOne(x => x.Supplier)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BookingNumber).IsRequired();
        });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.State).IsRequired();

            entity.HasOne(x => x.Booking)
                .WithOne(x => x.Claim)
                .HasForeignKey<Claim>(x => x.BookingId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ClaimDate)
                .WithOne(x => x.Claim)
                .HasForeignKey<ClaimDate>(x => x.ClaimId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Compensation)
                .WithOne(x => x.Claim)
                .HasForeignKey<Compensation>(x => x.ClaimId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClaimDate>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClaimId).IsRequired();
            entity.HasIndex(x => x.DateOfReceivedClaim);
        });

        modelBuilder.Entity<Compensation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClaimId).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).IsRequired();
            entity.Property(x => x.LastName).IsRequired();
            entity.Property(x => x.Email).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Role).IsRequired();
        });
    }
}
