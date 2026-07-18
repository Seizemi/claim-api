using FluentValidation.TestHelper;
using Modules.Claims.Features.Features.GetAllClaims;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Tests.Features.GetAllClaims;

[TestClass]
public sealed class GetAllClaimsRequestValidatorTests
{
    private readonly GetAllClaimsRequestValidator _validator = new();

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Validate_WithPageNumberLessThanOne_HasValidationErrorForPageNumber(int pageNumber)
    {
        // Arrange
        var request = new GetAllClaimsRequest(PageNumber: pageNumber, PageSize: 20);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageNumber)
            .WithErrorCode(PagingErrorCodes.PageNumberMustBeGreaterThanZero);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(101)]
    public void Validate_WithPageSizeOutOfRange_HasValidationErrorForPageSize(int pageSize)
    {
        // Arrange
        var request = new GetAllClaimsRequest(PageNumber: 1, PageSize: pageSize);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PageSize)
            .WithErrorCode(PagingErrorCodes.PageSizeMustBeBetweenOneAndMax);
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(1, 100)]
    [DataRow(5, 20)]
    public void Validate_WithValuesWithinRange_HasNoValidationErrors(int pageNumber, int pageSize)
    {
        // Arrange
        var request = new GetAllClaimsRequest(PageNumber: pageNumber, PageSize: pageSize);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
