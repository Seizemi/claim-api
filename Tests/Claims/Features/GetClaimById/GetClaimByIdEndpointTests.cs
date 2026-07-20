using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Modules.Claims.Features.Features.GetClaimById;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Features.Shared.Routes;
using Modules.Claims.Features.Tests.Shared;
using Modules.Common.Features;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetClaimById;

public sealed class GetClaimByIdEndpointTests
{
    [Fact]
    public void AddRoutes_Always_RegistersGetOnClaimDetailsRouteWithRouteName()
    {
        // Arrange
        var endpoint = new GetClaimByIdEndpoint();

        // Act
        var routeEndpoint = EndpointRouteTestHelper.MapSingleEndpoint(endpoint, services =>
        {
            services.AddSingleton(Mock.Of<IValidator<GetClaimByIdRequest>>());
            services.AddSingleton(Mock.Of<IGetClaimByIdHandler>());
        });

        // Assert
        Assert.Equal("/api/v1.0/Claim/claim-details/{claimId}/information", routeEndpoint.RoutePattern.RawText);

        var httpMethodMetadata = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        Assert.NotNull(httpMethodMetadata);
        Assert.Contains("GET", httpMethodMetadata.HttpMethods);

        var endpointNameMetadata = routeEndpoint.Metadata.GetMetadata<IEndpointNameMetadata>();
        Assert.NotNull(endpointNameMetadata);
        Assert.Equal(RouteConsts.GetClaimByIdRouteName, endpointNameMetadata.EndpointName);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsValidationProblemWithoutCallingHandler()
    {
        // Arrange
        var claimId = Guid.Empty;

        var validatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(GetClaimByIdRequest.ClaimId), "Claim id cannot be empty.")
            ]));

        var handlerMock = new Mock<IGetClaimByIdHandler>();

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetClaimByIdEndpoint),
            claimId,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var problem = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenHandlerReturnsError_ReturnsMappedProblem()
    {
        // Arrange
        var claimId = Guid.CreateVersion7();

        var validatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<IGetClaimByIdHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Validation(ClaimErrorCodes.ClaimCannotBeNull, ClaimErrorMessages.ClaimCannotBeNull));

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetClaimByIdEndpoint),
            claimId,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var problem = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
    }

    [Fact]
    public async Task Handle_WhenValidAndHandlerSucceeds_ReturnsOkWithClaimResponse()
    {
        // Arrange
        var claimId = Guid.CreateVersion7();
        var claimResponse = new ClaimResponse(
            claimId,
            default,
            null, null, null, null, null, null, null, null,
            null!, null!, null!);

        var validatorMock = new Mock<IValidator<GetClaimByIdRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(new GetClaimByIdRequest(claimId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<IGetClaimByIdHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(claimId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimResponse);

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetClaimByIdEndpoint),
            claimId,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<ClaimResponse>>(result);
        Assert.Same(claimResponse, okResult.Value);
    }
}
