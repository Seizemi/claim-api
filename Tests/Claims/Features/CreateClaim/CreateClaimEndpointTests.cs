using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Modules.Claims.Features.Features.CreateClaim;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Routes;
using Modules.Claims.Features.Tests.Shared;
using Modules.Common.Features;

namespace Modules.Claims.Features.Tests.Features.CreateClaim;

[TestClass]
public sealed class CreateClaimEndpointTests
{
    [TestMethod]
    public void AddRoutes_Always_RegistersPostOnNewClaimRoute()
    {
        // Arrange
        var endpoint = new CreateClaimEndpoint();

        // Act
        var routeEndpoint = EndpointRouteTestHelper.MapSingleEndpoint(endpoint, services =>
        {
            services.AddSingleton(Mock.Of<IValidator<ClaimRequest>>());
            services.AddSingleton(Mock.Of<ICreateClaimHandler>());
        });

        // Assert
        Assert.AreEqual("/api/v1.0/Claim/new-claim/claim", routeEndpoint.RoutePattern.RawText);
        var httpMethodMetadata = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        Assert.IsNotNull(httpMethodMetadata);
        Assert.Contains("POST", httpMethodMetadata.HttpMethods);
    }

    [TestMethod]
    public async Task Handle_WhenValidationFails_ReturnsValidationProblemWithoutCallingHandler()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();

        var validatorMock = new Mock<IValidator<ClaimRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(ClaimRequest.Booking), "Booking cannot be null.")
            ]));

        var handlerMock = new Mock<ICreateClaimHandler>();

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(CreateClaimEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<IStatusCodeHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status400BadRequest, problem.StatusCode);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<ClaimRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenHandlerReturnsError_ReturnsMappedProblem()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();

        var validatorMock = new Mock<IValidator<ClaimRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<ICreateClaimHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure("CreateClaim.Unexpected", "boom"));

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(CreateClaimEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<IStatusCodeHttpResult>(result, out var problem);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }

    [TestMethod]
    public async Task Handle_WhenValidAndHandlerSucceeds_ReturnsCreatedAtRouteWithClaimId()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        var claimId = Guid.CreateVersion7();

        var validatorMock = new Mock<IValidator<ClaimRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<ICreateClaimHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimId);

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(CreateClaimEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<CreatedAtRoute<Guid>>(result, out var created);
        Assert.AreEqual(RouteConsts.GetClaimByIdRouteName, created.RouteName);
        Assert.AreEqual(claimId, created.Value);
        Assert.AreEqual(claimId, created.RouteValues["claimId"]);
    }
}
