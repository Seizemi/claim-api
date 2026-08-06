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
    public DbSet<Reason> Reasons { get; set; }
    public DbSet<Solution> Solutions { get; set; }
    public DbSet<FollowedBy> FollowedBies { get; set; }
    public DbSet<RefundState> RefundStates { get; set; }
    public DbSet<CompensationReason> CompensationReasons { get; set; }
    public DbSet<SalesChannel> SalesChannels { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<SkissimType> SkissimTypes { get; set; }

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
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
            entity.Property(x => x.SupplierAkioNumber).IsRequired();

            entity.HasMany(x => x.Bookings)
                .WithOne(x => x.Supplier)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Service)
                .WithMany(x => x.Suppliers)
                .HasForeignKey(x => x.ServiceId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BookingNumber).IsRequired();

            entity.HasOne(x => x.SalesChannel)
                .WithMany()
                .HasForeignKey(x => x.SalesChannelId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SkissimType)
                .WithMany()
                .HasForeignKey(x => x.SkissimTypeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
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

            entity.HasOne(x => x.Reason)
                .WithMany()
                .HasForeignKey(x => x.ReasonId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Solution)
                .WithMany()
                .HasForeignKey(x => x.SolutionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FollowedBy)
                .WithMany()
                .HasForeignKey(x => x.FollowedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClaimDate>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClaimId).IsRequired();
            entity.HasIndex(x => x.DateOfReceivedClaim);
            entity.HasIndex(x => x.DateOfArrival);
        });

        modelBuilder.Entity<Compensation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ClaimId).IsRequired();

            entity.HasOne(x => x.RefundState)
                .WithMany()
                .HasForeignKey(x => x.RefundStateId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompensationReason)
                .WithMany()
                .HasForeignKey(x => x.CompensationReasonId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<Reason>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
        });

        modelBuilder.Entity<Solution>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
        });

        modelBuilder.Entity<FollowedBy>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
        });

        modelBuilder.Entity<RefundState>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
        });

        modelBuilder.Entity<CompensationReason>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
        });

        modelBuilder.Entity<SalesChannel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
            entity.Property(x => x.Language).IsRequired();
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
        });

        modelBuilder.Entity<SkissimType>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Label).IsRequired();
            entity.Property(x => x.Value).IsRequired();
        });
    }
}
