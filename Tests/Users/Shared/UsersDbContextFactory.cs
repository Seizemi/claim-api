using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Enums;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Features.Tests.Shared;

internal static class UsersDbContextFactory
{
    /// <summary>A context on a fresh in-memory database; pass the same name to share it between contexts.</summary>
    internal static UsersDbContext Create(string? databaseName = null) =>
        new(Options(databaseName ?? Guid.NewGuid().ToString()));

    internal static DbContextOptions<UsersDbContext> Options(string databaseName) =>
        new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

    internal static PendingRoleAssignment PendingAssignment(
        Guid? userId = null,
        string displayName = "Ann Agent",
        RoleAssignmentStatus status = RoleAssignmentStatus.Pending,
        DateTimeOffset? nextAttemptAt = null,
        int attempts = 0) => new()
    {
        UserId = userId ?? Guid.NewGuid(),
        Email = "ann@example.com",
        DisplayName = displayName,
        Role = "Agent",
        CreatedBy = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
        CreatedAt = DateTimeOffset.UnixEpoch,
        Status = status,
        Attempts = attempts,
        NextAttemptAt = nextAttemptAt ?? DateTimeOffset.UnixEpoch,
        Version = Guid.NewGuid()
    };
}
