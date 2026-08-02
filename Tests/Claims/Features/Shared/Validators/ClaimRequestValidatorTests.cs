using FluentValidation.TestHelper;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Validators;
using Modules.Claims.Features.Tests.Shared;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.Shared.Validators;

public sealed class ClaimRequestValidatorTests
{
    private readonly ClaimRequestValidator _validator = new();

    [Fact]
    public void Validate_WithNullBooking_HasValidationErrorForBooking()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest() with { Booking = null! };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Booking)
            .WithErrorCode(BookingErrorCodes.BookingCannotBeNull);
    }

    [Fact]
    public void Validate_WithBookingNumberEmpty_HasValidationErrorForNestedBookingNumber()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Booking = request.Booking with { BookingNumber = string.Empty } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.BookingNumber")
            .WithErrorCode(ClaimErrorCodes.ClaimBookingNumberCannotBeNullOrEmpty);
    }

    [Fact]
    public void Validate_WithNullCustomer_HasValidationErrorForNestedCustomer()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Booking = request.Booking with { Customer = null! } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.Customer")
            .WithErrorCode(CustomerErrorCodes.CustomerCannotBeNull);
    }

    [Fact]
    public void Validate_WithCustomerNameEmpty_HasValidationErrorForNestedCustomerName()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with
        {
            Booking = request.Booking with
            {
                Customer = request.Booking.Customer with { Name = string.Empty }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.Customer.Name")
            .WithErrorCode(ClaimErrorCodes.ClaimCustomerNameCannotBeNullOrEmpty);
    }

    [Fact]
    public void Validate_WithNullSupplier_HasValidationErrorForNestedSupplier()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Booking = request.Booking with { Supplier = null! } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.Supplier")
            .WithErrorCode(SupplierErrorCodes.SupplierCannotBeNull);
    }

    [Fact]
    public void Validate_WithSupplierIdEmpty_HasValidationErrorForNestedSupplierId()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with
        {
            Booking = request.Booking with
            {
                Supplier = request.Booking.Supplier with { Id = Guid.Empty }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.Supplier.Id")
            .WithErrorCode(ClaimErrorCodes.ClaimSupplierIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithReasonIdEmpty_HasValidationErrorForReasonId()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest() with { ReasonId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReasonId)
            .WithErrorCode(ClaimErrorCodes.ClaimReasonIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithSolutionIdEmpty_HasValidationErrorForSolutionId()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest() with { SolutionId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SolutionId)
            .WithErrorCode(ClaimErrorCodes.ClaimSolutionIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithSalesChannelIdEmpty_HasValidationErrorForNestedSalesChannelId()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Booking = request.Booking with { SalesChannelId = Guid.Empty } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.SalesChannelId")
            .WithErrorCode(ClaimErrorCodes.ClaimSalesChannelIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithSkissimTypeIdEmpty_HasValidationErrorForNestedSkissimTypeId()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Booking = request.Booking with { SkissimTypeId = Guid.Empty } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Booking.SkissimTypeId")
            .WithErrorCode(ClaimErrorCodes.ClaimSkissimTypeIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithNullCompensation_HasValidationErrorForCompensation()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest() with { Compensation = null! };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Compensation)
            .WithErrorCode(CompensationErrorCodes.CompensationCannotBeNull);
    }

    [Fact]
    public void Validate_WithRefundStateIdEmpty_HasValidationErrorForNestedRefundStateId()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Compensation = request.Compensation with { RefundStateId = Guid.Empty } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Compensation.RefundStateId")
            .WithErrorCode(ClaimErrorCodes.ClaimRefundStateIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithCompensationReasonIdEmpty_HasValidationErrorForNestedCompensationReasonId()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { Compensation = request.Compensation with { CompensationReasonId = Guid.Empty } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("Compensation.CompensationReasonId")
            .WithErrorCode(ClaimErrorCodes.ClaimCompensationReasonIdCannotBeEmpty);
    }

    [Fact]
    public void Validate_WithDepartureDateBeforeArrivalDate_HasValidationErrorForClaimDate()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with
        {
            ClaimDate = request.ClaimDate with
            {
                DateOfDeparture = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
                DateOfArrival = DateOnly.FromDateTime(DateTime.UtcNow)
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("ClaimDate")
            .WithErrorCode(ClaimErrorCodes.ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival);
    }

    [Fact]
    public void Validate_WithNullDateOfArrival_HasValidationErrorForNestedDateOfArrival()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();
        request = request with { ClaimDate = request.ClaimDate with { DateOfArrival = null } };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor("ClaimDate.DateOfArrival")
            .WithErrorCode(ClaimErrorCodes.ClaimDateOfArrivalCannotBeNull);
    }

    [Fact]
    public void Validate_WithNullClaimDate_HasNoValidationErrorForClaimDate()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest() with { ClaimDate = null! };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ClaimDate);
    }

    [Fact]
    public void Validate_WithValidRequest_HasNoValidationErrors()
    {
        // Arrange
        var request = ClaimTestDataFactory.CreateClaimRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
