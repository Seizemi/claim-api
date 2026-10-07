using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Modules.Claims.Features.Features.ReleaseClaimLock;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Tests.Shared;
using Modules.Common.Features.Authorization;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.ReleaseClaimLock;

public sealed class ReleaseClaimLockEndpointTests
{
    private static readonly CurrentUser CurrentUser = new(Guid.CreateVersion7(), "Alice");

    [Fact]
    public async Task Handle_WhenClaimIdValidationFails_ReturnsValidationProblemWithoutCallingHandler()
    {
        // Arrange
        var claimId = Guid.Empty;
        var claimIdValidatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        claimIdValidatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(GetClaimByIdRequest.ClaimId), "Claim id cannot be empty.")
            ]));
        var handlerMock = new Mock<IReleaseClaimLockHandler>();

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(ReleaseClaimLockEndpoint),
            claimId,
            CurrentUser,
            claimIdValidatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var problem = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValid_ReleasesTheSessionUsersLockAndReturnsNoContent()
    {
        // Arrange
        var claimId = Guid.CreateVersion7();
        var claimIdValidatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        claimIdValidatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var handlerMock = new Mock<IReleaseClaimLockHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(claimId, CurrentUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(ReleaseClaimLockEndpoint),
            claimId,
            CurrentUser,
            claimIdValidatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsType<NoContent>(result);
        handlerMock.Verify(h => h.HandleAsync(claimId, CurrentUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
