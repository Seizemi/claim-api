using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Modules.Common.Features.Authorization;
using Modules.Users.Domain.Enums;
using Modules.Users.Features.Features.DeleteUser;
using Modules.Users.Features.Features.Shared.Errors;
using Modules.Users.Features.Tests.Shared;
using Modules.Users.Infrastructure.Database;
using Moq;
using Xunit;

namespace Modules.Users.Features.Tests.Features.DeleteUser;

public sealed class DeleteUserHandlerTests
{
    private static readonly CurrentUser Supervisor = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), "Sam Supervisor");
    private static readonly Guid TargetId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

    private readonly Mock<IPartnerUserService> _partnerUsers = new();
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly UsersDbContext _dbContext;
    private readonly FakeLogger<DeleteUserHandler> _logger = new();
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _dbContext = UsersDbContextFactory.Create(_databaseName);
        _handler = new DeleteUserHandler(_partnerUsers.Object, _dbContext, _logger);
    }

    [Fact]
    public async Task HandleAsync_WithAgent_DeletesAndWritesAuditEvent()
    {
        GivenTarget(AppRoles.Agent);

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        _partnerUsers.Verify(service => service.DeleteUserAsync(TargetId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        var audit = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Information, audit.Level);
        Assert.Contains(TargetId.ToString(), audit.Message);
    }

    [Fact]
    public async Task HandleAsync_WithOwnId_ReturnsValidationErrorWithoutCallingGraph()
    {
        var result = await _handler.HandleAsync(Supervisor.Id, Supervisor, TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal(UserErrorCodes.CannotDeleteSelf, result.FirstError.Code);
        _partnerUsers.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_WithSupervisor_ReturnsValidationErrorAndDoesNotDelete()
    {
        GivenTarget(AppRoles.Supervisor);

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal(UserErrorCodes.CannotDeleteSupervisor, result.FirstError.Code);
        _partnerUsers.Verify(service => service.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(LogLevel.Warning, Assert.Single(_logger.Collector.GetSnapshot()).Level);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownUser_ReturnsNotFound()
    {
        _partnerUsers.Setup(service => service.GetUserAsync(TargetId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PartnerUser?)null);

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        _partnerUsers.Verify(service => service.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithTenantUserNotAssignedToApp_ReturnsNotFoundAndDoesNotDelete()
    {
        GivenTarget(role: null);

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        _partnerUsers.Verify(service => service.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDisappearsBeforeDelete_ReturnsNotFound()
    {
        GivenTarget(AppRoles.Agent);
        _partnerUsers.Setup(service => service.DeleteUserAsync(TargetId.ToString(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.UserNotFound, "gone"));

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal(UserErrorCodes.NotFound, result.FirstError.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenGraphFails_Returns502ErrorAndLogsAuditFailure()
    {
        _partnerUsers.Setup(service => service.GetUserAsync(TargetId.ToString(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.GraphError, "throttled"));

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status502BadGateway, (int)result.FirstError.Type);
        var audit = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, audit.Level);
    }

    [Theory]
    [InlineData(RoleAssignmentStatus.Pending)]
    [InlineData(RoleAssignmentStatus.Failed)]
    public async Task HandleAsync_WithUserWaitingForRole_DeletesUserAndRow(RoleAssignmentStatus status)
    {
        await GivenPendingTarget(status);

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        _partnerUsers.Verify(service => service.DeleteUserAsync(TargetId.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        _partnerUsers.Verify(service => service.GetUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Empty(await _dbContext.PendingRoleAssignments.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(LogLevel.Information, Assert.Single(_logger.Collector.GetSnapshot()).Level);
    }

    [Fact]
    public async Task HandleAsync_WithUserWaitingForRoleAlreadyGoneFromEntra_DeletesTheRow()
    {
        await GivenPendingTarget(RoleAssignmentStatus.Failed);
        _partnerUsers.Setup(service => service.DeleteUserAsync(TargetId.ToString(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.UserNotFound, "gone"));

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Empty(await _dbContext.PendingRoleAssignments.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WithUserWaitingForRoleWhenGraphFails_Returns502AndKeepsTheRow()
    {
        await GivenPendingTarget(RoleAssignmentStatus.Pending);
        _partnerUsers.Setup(service => service.DeleteUserAsync(TargetId.ToString(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.GraphError, "throttled"));

        var result = await _handler.HandleAsync(TargetId, Supervisor, TestContext.Current.CancellationToken);

        Assert.Equal(StatusCodes.Status502BadGateway, (int)result.FirstError.Type);
        Assert.Single(await _dbContext.PendingRoleAssignments.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
    }

    private async Task GivenPendingTarget(RoleAssignmentStatus status)
    {
        await using var seed = UsersDbContextFactory.Create(_databaseName);
        seed.PendingRoleAssignments.Add(UsersDbContextFactory.PendingAssignment(TargetId, status: status));
        await seed.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private void GivenTarget(string? role) =>
        _partnerUsers.Setup(service => service.GetUserAsync(TargetId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PartnerUser(TargetId.ToString(), "Target", "target@example.com", role));
}
