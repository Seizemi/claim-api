using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.UpdateClaim;
using Modules.Claims.Features.Tests.Shared;
using Modules.Common.Features;

namespace Modules.Claims.Features.Tests.Features.UpdateClaim;

[TestClass]
public sealed class UpdateClaimEndpointTests
{
    [TestMethod]
    public void AddRoutes_Always_RegistersPutOnClaimDetailsRoute()
    {
        // Arrange
        var endpoint = new UpdateClaimEndpoint();

        // Act
        var routeEndpoint = EndpointRouteTestHelper.MapSingleEndpoint(endpoint, services =>
        {
            services.AddSingleton(Mock.Of<IValidator<GetClaimByIdRequest>>());
            services.AddSingleton(Mock.Of<IValidator<ClaimRequest>>());
            services.AddSingleton(Mock.Of<IUpdateClaimHandler>());
        });

        // Assert
        Assert.AreEqual("/api/v1.0/Claim/claim-details/{claimId}/information", routeEndpoint.RoutePattern.RawText);
        var httpMethodMetadata = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        Assert.IsNotNull(httpMethodMetadata);
        Assert.Contains("PUT", httpMethodMetadata.HttpMethods);
    }

    [TestMethod]
    public async Task Handle_WhenClaimIdValidationFails_ReturnsValidationProblemWithoutValidatingRequestOrCallingHandler()
    {
        // Arrange
        var claimId = Guid.Empty;
        var request = ClaimTestDataFactory.CreateClaimRequest();

        var claimIdValidatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        claimIdValidatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(GetClaimByIdRequest.ClaimId), "Claim id cannot be empty.")
            ]));

        var requestValidatorMock = new Mock<IValidator<ClaimRequest>>();
        var handlerMock = new Mock<IUpdateClaimHandler>();

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(UpdateClaimEndpoint),
            claimId,
            request,
            claimIdValidatorMock.Object,
            requestValidatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<IStatusCodeHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.StatusCode);
        requestValidatorMock.Verify(
            v => v.ValidateAsync(It.IsAny<ClaimRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<Guid>(), It.IsAny<ClaimRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenClaimIdValidButRequestValidationFails_ReturnsValidationProblemWithoutCallingHandler()
    {
        // Arrange
        var claimId = Guid.CreateVersion7();
        var request = ClaimTestDataFactory.CreateClaimRequest();

        var claimIdValidatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        claimIdValidatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var requestValidatorMock = new Mock<IValidator<ClaimRequest>>();
        requestValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(ClaimRequest.Booking), "Booking cannot be null.")
            ]));

        var handlerMock = new Mock<IUpdateClaimHandler>();

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(UpdateClaimEndpoint),
            claimId,
            request,
            claimIdValidatorMock.Object,
            requestValidatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<IStatusCodeHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.StatusCode);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<Guid>(), It.IsAny<ClaimRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenHandlerReturnsError_ReturnsMappedProblem()
    {
        // Arrange
        var claimId = Guid.CreateVersion7();
        var request = ClaimTestDataFactory.CreateClaimRequest();

        var claimIdValidatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        claimIdValidatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var requestValidatorMock = new Mock<IValidator<ClaimRequest>>();
        requestValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<IUpdateClaimHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(claimId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation(ClaimErrorCodes.ClaimCannotBeNull, ClaimErrorMessages.ClaimCannotBeNull));

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(UpdateClaimEndpoint),
            claimId,
            request,
            claimIdValidatorMock.Object,
            requestValidatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<IStatusCodeHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [TestMethod]
    public async Task Handle_WhenValidAndHandlerSucceeds_ReturnsOk()
    {
        // Arrange
        var claimId = Guid.CreateVersion7();
        var request = ClaimTestDataFactory.CreateClaimRequest();

        var claimIdValidatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        claimIdValidatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var requestValidatorMock = new Mock<IValidator<ClaimRequest>>();
        requestValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<IUpdateClaimHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(claimId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Updated);

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(UpdateClaimEndpoint),
            claimId,
            request,
            claimIdValidatorMock.Object,
            requestValidatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<Ok>(result, out var okResult);
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
    }
}
