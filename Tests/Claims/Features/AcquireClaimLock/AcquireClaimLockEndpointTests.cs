using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Modules.Claims.Features.Features.AcquireClaimLock;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Tests.Shared;
using Modules.Common.Features.Authorization;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.AcquireClaimLock;

public sealed class AcquireClaimLockEndpointTests
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
        var handlerMock = new Mock<IAcquireClaimLockHandler>();

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(AcquireClaimLockEndpoint),
            claimId,
            CurrentUser,
            claimIdValidatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var problem = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<Guid>(), It.IsAny<CurrentUser>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValid_LocksForTheSessionUserAndReturnsOk()
    {
        // Arrange
        var claimId = Guid.CreateVersion7();
        var claimIdValidatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        claimIdValidatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        var response = new AcquireClaimLockResponse(false, Guid.CreateVersion7(), "Bob", DateTimeOffset.UtcNow, Claim: null);
        var handlerMock = new Mock<IAcquireClaimLockHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(claimId, CurrentUser, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(AcquireClaimLockEndpoint),
            claimId,
            CurrentUser,
            claimIdValidatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<AcquireClaimLockResponse>>(result);
        Assert.Equal(response, okResult.Value);
        handlerMock.Verify(h => h.HandleAsync(claimId, CurrentUser, It.IsAny<CancellationToken>()), Times.Once);
    }
}
