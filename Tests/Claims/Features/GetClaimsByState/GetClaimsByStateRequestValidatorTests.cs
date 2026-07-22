using FluentValidation.TestHelper;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.GetClaimsByState;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.GetClaimsByState;

public sealed class GetClaimsByStateRequestValidatorTests
{
    private readonly GetClaimsByStateRequestValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithPageNumberLessThanOne_HasValidationErrorForPageNumber(int pageNumber)
    {
        // Arrange
        var request = new GetClaimsByStateRequest(ClaimState.AwaitingSupplier, PageNumber: pageNumber, PageSize: 20);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorCode(PagingErrorCodes.PageNumberMustBeGreaterThanZero);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithPageSizeOutOfRange_HasValidationErrorForPageSize(int pageSize)
    {
        // Arrange
        var request = new GetClaimsByStateRequest(ClaimState.AwaitingSupplier, PageNumber: 1, PageSize: pageSize);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorCode(PagingErrorCodes.PageSizeMustBeBetweenOneAndMax);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 100)]
    [InlineData(5, 20)]
    public void Validate_WithValuesWithinRange_HasNoValidationErrors(int pageNumber, int pageSize)
    {
        // Arrange
        var request = new GetClaimsByStateRequest(ClaimState.AwaitingSupplier, PageNumber: pageNumber, PageSize: pageSize);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
