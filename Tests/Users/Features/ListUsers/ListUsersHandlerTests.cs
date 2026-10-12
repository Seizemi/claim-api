using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Enums;
using Modules.Users.Features.Features.ListUsers;
using Modules.Users.Features.Features.Shared.Responses;
using Modules.Users.Features.Tests.Shared;
using Moq;
using Xunit;

namespace Modules.Users.Features.Tests.Features.ListUsers;

public sealed class ListUsersHandlerTests
{
    private readonly Mock<IPartnerUserService> _partnerUsers = new();
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly ListUsersHandler _handler;

    public ListUsersHandlerTests()
    {
        _partnerUsers.Setup(service => service.ListUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _handler = new ListUsersHandler(_partnerUsers.Object, UsersDbContextFactory.Create(_databaseName), NullLogger<ListUsersHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WithUsers_ReturnsThemActiveSortedByDisplayName()
    {
        _partnerUsers.Setup(service => service.ListUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new PartnerUser("2", "zoe", "zoe@example.com", "Agent"),
                new PartnerUser("1", "Adam", "adam@example.com", "Supervisor"),
                new PartnerUser("3", "Marc", "marc@example.com", null)
            ]);

        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(["Adam", "Marc", "zoe"], result.Value.Select(user => user.DisplayName));
        Assert.Equal(new UserResponse("1", "Adam", "adam@example.com", "Supervisor", UserStatus.Active), result.Value[0]);
        Assert.Null(result.Value[1].Role);
        Assert.All(result.Value, user => Assert.Equal(UserStatus.Active, user.Status));
    }

    [Fact]
    public async Task HandleAsync_WithUsersWaitingForRole_ListsThemPendingOrFailedAmongTheOthers()
    {
        var pendingId = Guid.NewGuid();
        var failedId = Guid.NewGuid();
        await SeedAsync(
            UsersDbContextFactory.PendingAssignment(pendingId, "Bob"),
            UsersDbContextFactory.PendingAssignment(failedId, "Ann", RoleAssignmentStatus.Failed));
        _partnerUsers.Setup(service => service.ListUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PartnerUser(Guid.NewGuid().ToString(), "Carl", "carl@example.com", "Agent")]);

        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["Ann", "Bob", "Carl"], result.Value.Select(user => user.DisplayName));
        Assert.Equal(new UserResponse(failedId.ToString(), "Ann", "ann@example.com", "Agent", UserStatus.Failed), result.Value[0]);
        Assert.Equal(new UserResponse(pendingId.ToString(), "Bob", "ann@example.com", "Agent", UserStatus.Pending), result.Value[1]);
        Assert.Equal(UserStatus.Active, result.Value[2].Status);
    }

    [Fact]
    public async Task HandleAsync_WithUserJustAssigned_ListsThemOnceAsActive()
    {
        var userId = Guid.NewGuid();
        await SeedAsync(UsersDbContextFactory.PendingAssignment(userId, "Ann"));
        _partnerUsers.Setup(service => service.ListUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PartnerUser(userId.ToString(), "Ann", "ann@example.com", "Agent")]);

        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        var user = Assert.Single(result.Value);
        Assert.Equal(UserStatus.Active, user.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenGraphFails_Returns502Error()
    {
        _partnerUsers.Setup(service => service.ListUsersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.GraphError, "down"));

        var result = await _handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Equal(StatusCodes.Status502BadGateway, (int)result.FirstError.Type);
    }

    private async Task SeedAsync(params PendingRoleAssignment[] rows)
    {
        await using var seed = UsersDbContextFactory.Create(_databaseName);
        seed.PendingRoleAssignments.AddRange(rows);
        await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
