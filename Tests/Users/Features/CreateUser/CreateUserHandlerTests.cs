using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Modules.Common.Features.Authorization;
using Modules.Users.Domain.Enums;
using Modules.Users.Features.Features.CreateUser;
using Modules.Users.Features.Features.Shared.Errors;
using Modules.Users.Features.Features.Shared.Requests;
using Modules.Users.Features.Features.Shared.Responses;
using Modules.Users.Features.RoleAssignment;
using Modules.Users.Features.Tests.Shared;
using Modules.Users.Infrastructure.Database;
using Moq;
using Xunit;

namespace Modules.Users.Features.Tests.Features.CreateUser;

public sealed class CreateUserHandlerTests
{
    private static readonly CurrentUser Supervisor = new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), "Sam Supervisor");
    private static readonly CreateUserRequest Request = new("agent@example.com", "Ann Agent", "Agent");
    private static readonly PartnerUser CreatedUser = new("bbbbbbbb-0000-0000-0000-000000000002", "Ann Agent", "agent@example.com", Role: null);

    private readonly Mock<IPartnerUserService> _partnerUsers = new();
    private readonly UsersDbContext _dbContext = UsersDbContextFactory.Create();
    private readonly RoleAssignmentSignal _signal = new();
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 10, 9, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeLogger<CreateUserHandler> _logger = new();

    private CreateUserHandler CreateHandler(UsersDbContext? dbContext = null) =>
        new(_partnerUsers.Object, dbContext ?? _dbContext, _signal, _time, _logger);

    private void GivenUserCreated() =>
        _partnerUsers.Setup(service => service.CreateUserAsync(It.IsAny<CreatePartnerUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreatedUser);

    [Fact]
    public async Task HandleAsync_WhenCreated_ReturnsPendingUserWithRequestedRole()
    {
        GivenUserCreated();

        var result = await CreateHandler().HandleAsync(Request, Supervisor, TestContext.Current.CancellationToken);

        Assert.False(result.IsError);
        Assert.Equal(new UserResponse(CreatedUser.Id, "Ann Agent", "agent@example.com", "Agent", UserStatus.Pending), result.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenCreated_RecordsRoleToAssignNowAndWakesTheWorker()
    {
        GivenUserCreated();

        await CreateHandler().HandleAsync(Request, Supervisor, TestContext.Current.CancellationToken);

        var row = Assert.Single(await _dbContext.PendingRoleAssignments.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(Guid.Parse(CreatedUser.Id), row.UserId);
        Assert.Equal("Agent", row.Role);
        Assert.Equal("agent@example.com", row.Email);
        Assert.Equal("Ann Agent", row.DisplayName);
        Assert.Equal(Supervisor.Id, row.CreatedBy);
        Assert.Equal(RoleAssignmentStatus.Pending, row.Status);
        Assert.Equal(0, row.Attempts);
        Assert.Equal(_time.GetUtcNow(), row.CreatedAt);
        Assert.Equal(_time.GetUtcNow(), row.NextAttemptAt);
        Assert.Null(row.LockedUntil);

        // Notified: the wait returns at once instead of after its timeout.
        var wait = _signal.WaitAsync(TimeSpan.FromHours(1), TimeProvider.System, TestContext.Current.CancellationToken);
        Assert.True(wait.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task HandleAsync_WithPaddedDisplayName_PassesItTrimmed()
    {
        GivenUserCreated();

        await CreateHandler().HandleAsync(Request with { DisplayName = "  Ann Agent " }, Supervisor, TestContext.Current.CancellationToken);

        _partnerUsers.Verify(service => service.CreateUserAsync(
            new CreatePartnerUserRequest("agent@example.com", "Ann Agent", "Agent"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCreated_WritesAuditEventWithoutEmail()
    {
        GivenUserCreated();

        await CreateHandler().HandleAsync(Request, Supervisor, TestContext.Current.CancellationToken);

        var audit = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Information, audit.Level);
        Assert.Contains(Supervisor.Id.ToString(), audit.Message);
        Assert.Contains(CreatedUser.Id, audit.Message);
        Assert.DoesNotContain(CreatedUser.Email, audit.Message);
    }

    [Theory]
    [InlineData(PartnerUserErrorCode.UserAlreadyExists, ErrorType.Conflict, UserErrorCodes.AlreadyExists)]
    [InlineData(PartnerUserErrorCode.UnknownRole, ErrorType.Validation, UserErrorCodes.RoleIsNotAssignable)]
    [InlineData(PartnerUserErrorCode.GraphError, (ErrorType)StatusCodes.Status502BadGateway, UserErrorCodes.DirectoryUnavailable)]
    public async Task HandleAsync_WhenPackageFails_ReturnsMappedErrorAndRecordsNothing(
        PartnerUserErrorCode code, ErrorType expectedType, string expectedCode)
    {
        _partnerUsers.Setup(service => service.CreateUserAsync(It.IsAny<CreatePartnerUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(code, "Graph said no"));

        var result = await CreateHandler().HandleAsync(Request, Supervisor, TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Equal(expectedType, result.FirstError.Type);
        Assert.Equal(expectedCode, result.FirstError.Code);
        Assert.DoesNotContain("Graph said no", result.FirstError.Description);
        Assert.Empty(await _dbContext.PendingRoleAssignments.ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_WhenGraphFails_LogsAuditFailureAsError()
    {
        _partnerUsers.Setup(service => service.CreateUserAsync(It.IsAny<CreatePartnerUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.GraphError, "Graph said no"));

        await CreateHandler().HandleAsync(Request, Supervisor, TestContext.Current.CancellationToken);

        var audit = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, audit.Level);
        Assert.IsType<PartnerUserException>(audit.Exception);
    }

    [Fact]
    public async Task HandleAsync_WhenRowCannotBeSaved_DeletesTheCreatedUserAndReturnsUnexpected()
    {
        GivenUserCreated();
        await using var failingContext = new FailingUsersDbContext();

        var result = await CreateHandler(failingContext).HandleAsync(Request, Supervisor, TestContext.Current.CancellationToken);

        Assert.Equal(ErrorType.Unexpected, result.FirstError.Type);
        Assert.Equal(UserErrorCodes.CreationNotRecorded, result.FirstError.Code);
        _partnerUsers.Verify(service => service.DeleteUserAsync(CreatedUser.Id, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRowCannotBeSavedNorUserDeleted_LogsTheOrphanUser()
    {
        GivenUserCreated();
        _partnerUsers.Setup(service => service.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.GraphError, "down"));
        await using var failingContext = new FailingUsersDbContext();

        var result = await CreateHandler(failingContext).HandleAsync(Request, Supervisor, TestContext.Current.CancellationToken);

        Assert.True(result.IsError);
        Assert.Contains(_logger.Collector.GetSnapshot(), record =>
            record.Level == LogLevel.Error && record.Message.Contains(CreatedUser.Id) && record.Message.Contains("manually"));
    }

    private sealed class FailingUsersDbContext() : UsersDbContext(UsersDbContextFactory.Options(Guid.NewGuid().ToString()))
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Database unreachable.");
    }
}
