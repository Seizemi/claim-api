using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Modules.Claims.Features.Features.GetAllClaims;
using Modules.Claims.Features.Features.Shared.Requests;
using Modules.Claims.Features.Features.Shared.Responses;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetAllClaims;

public sealed class GetAllClaimsEndpointTests
{
    [Fact]
    public void AddRoutes_Always_RegistersGetOnDashboardClaimRoute()
    {
        // Arrange
        var endpoint = new GetAllClaimsEndpoint();

        // Act
        var routeEndpoint = EndpointRouteTestHelper.MapSingleEndpoint(endpoint, services =>
        {
            services.AddSingleton(Mock.Of<IValidator<GetAllClaimsRequest>>());
            services.AddSingleton(Mock.Of<IGetAllClaimsHandler>());
        });

        // Assert
        Assert.Equal("/api/v1.0/Claim/dashboard/claim", routeEndpoint.RoutePattern.RawText);
        var httpMethodMetadata = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
        Assert.NotNull(httpMethodMetadata);
        Assert.Contains("GET", httpMethodMetadata.HttpMethods);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsValidationProblemWithoutCallingHandler()
    {
        // Arrange
        var request = new GetAllClaimsRequest(PageNumber: 0, PageSize: 20);

        var validatorMock = new Mock<IValidator<GetAllClaimsRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure(nameof(GetAllClaimsRequest.PageNumber), "Page number must be greater than zero.")
            ]));

        var handlerMock = new Mock<IGetAllClaimsHandler>();

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetAllClaimsEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var problem = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.StatusCode);
        handlerMock.Verify(
            h => h.HandleAsync(It.IsAny<GetAllClaimsRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenHandlerReturnsError_ReturnsMappedProblem()
    {
        // Arrange
        var request = new GetAllClaimsRequest();

        var validatorMock = new Mock<IValidator<GetAllClaimsRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<IGetAllClaimsHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure("GetAllClaims.Unexpected", "boom"));

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetAllClaimsEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var problem = Assert.IsType<IStatusCodeHttpResult>(result, exactMatch: false);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.StatusCode);
    }

    [Fact]
    public async Task Handle_WhenValidAndHandlerSucceeds_ReturnsOkWithPagedResponse()
    {
        // Arrange
        var request = new GetAllClaimsRequest();
        var pagedResponse = new PagedResponse([], 1, 20, 0, 0);

        var validatorMock = new Mock<IValidator<GetAllClaimsRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handlerMock = new Mock<IGetAllClaimsHandler>();
        handlerMock
            .Setup(h => h.HandleAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await EndpointHandleInvoker.InvokeAsync(
            typeof(GetAllClaimsEndpoint),
            request,
            validatorMock.Object,
            handlerMock.Object,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<Ok<PagedResponse>>(result);
        Assert.Same(pagedResponse, okResult.Value);
    }
}
