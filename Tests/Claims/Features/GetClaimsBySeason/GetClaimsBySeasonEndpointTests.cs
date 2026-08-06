using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Modules.Claims.Features.Features.GetClaimsBySeason;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetClaimsBySeason;

public sealed class GetClaimsBySeasonEndpointTests
{
    [Fact]
    public void AddRoutes_Always_RegistersGetOnBySeasonRoute()
    {
        // Arrange
        var endpoint = new GetClaimsBySeasonEndpoint();

        // Act
        var routeEndpoint = EndpointRouteTestHelper.MapSingleEndpoint(endpoint, services =>
        {
            services.AddSingleton(Mock.Of<IValidator<GetClaimsBySeasonRequest>>());
            services.AddSingleton(Mock.Of<IGetClaimsBySeasonHandler>());
        });

        // Assert
        Assert.Equal("/api/v1.0/Claim/by-season/{seasonValue}", routeEndpoint.RoutePattern.RawText);
        var httpMethodMetadata = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        Assert.NotNull(httpMethodMetadata);
        Assert.Contains("GET", httpMethodMetadata.HttpMethods);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsValidationProblemWithoutCallingHandler()
    {
        // Arrange
        var request = new GetClaimsBySeasonRequest("not-a-season");

        var validatorMock = new Mock<IValidator<GetClaimsBySeasonRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(GetClaimsBySeasonRequest.SeasonValue), "Season value is invalid.")
            ]));

        var handlerMock = new Mock<IGetClaimsBySeasonHandler>();

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetClaimsBySeasonEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var problem = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<GetClaimsBySeasonRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenHandlerReturnsError_ReturnsMappedProblem()
    {
        // Arrange
        var request = new GetClaimsBySeasonRequest("ete2025");

        var validatorMock = new Mock<IValidator<GetClaimsBySeasonRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<IGetClaimsBySeasonHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure("GetClaimsBySeason.Unexpected", "boom"));

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetClaimsBySeasonEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var problem = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }

    [Fact]
    public async Task Handle_WhenValidAndHandlerSucceeds_ReturnsOkWithClaimList()
    {
        // Arrange
        var request = new GetClaimsBySeasonRequest("ete2025");
        IReadOnlyList<ClaimResponse> claims = [];

        var validatorMock = new Mock<IValidator<GetClaimsBySeasonRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<IGetClaimsBySeasonHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ErrorOrFactory.From(claims));

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetClaimsBySeasonEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<IReadOnlyList<ClaimResponse>>>(result);
        Assert.Same(claims, okResult.Value);
    }
}
