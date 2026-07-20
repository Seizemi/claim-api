using FluentValidation;
using Modules.Claims.Features.Features.Shared.Errors;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Features.Shared.Validators;

internal sealed class ClaimRequestValidator : AbstractValidator<ClaimRequest>
{
    public ClaimRequestValidator()
    {
        RuleFor(x => x.Booking)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(BookingErrorCodes.BookingCannotBeNull)
            .WithMessage(BookingErrorMessages.BookingCannotBeNull)
            .SetValidator(new BookingRequestValidator());

        RuleFor(x => x.ClaimDate)
            .SetValidator(new ClaimDateRequestValidator())
            .When(x => x.ClaimDate is not null);
    }
}

internal sealed class BookingRequestValidator : AbstractValidator<BookingRequest>
{
    public BookingRequestValidator()
    {
        RuleFor(x => x.BookingNumber)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimBookingNumberCannotBeNullOrEmpty)
            .WithMessage(ClaimErrorMessages.ClaimBookingNumberCannotBeNullOrEmpty);

        RuleFor(x => x.Customer)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(CustomerErrorCodes.CustomerCannotBeNull)
            .WithMessage(CustomerErrorMessages.CustomerCannotBeNull)
            .SetValidator(new CustomerRequestValidator());

        RuleFor(x => x.Supplier)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithErrorCode(SupplierErrorCodes.SupplierCannotBeNull)
            .WithMessage(SupplierErrorMessages.SupplierCannotBeNull)
            .SetValidator(new SupplierRequestValidator());
    }
}

internal sealed class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimCustomerNameCannotBeNullOrEmpty)
            .WithMessage(ClaimErrorMessages.ClaimCustomerNameCannotBeNullOrEmpty);
    }
}

internal sealed class SupplierRequestValidator : AbstractValidator<SupplierRequest>
{
    public SupplierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ClaimErrorCodes.ClaimSupplierNameCannotBeNullOrEmpty)
            .WithMessage(ClaimErrorMessages.ClaimSupplierNameCannotBeNullOrEmpty);
    }
}

internal sealed class ClaimDateRequestValidator : AbstractValidator<ClaimDateRequest>
{
    public ClaimDateRequestValidator()
    {
        RuleFor(x => x)
            .Must(cd => cd.DateOfDeparture is null
                     || cd.DateOfArrival is null
                     || cd.DateOfDeparture >= cd.DateOfArrival)
            .WithErrorCode(ClaimErrorCodes.ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival)
            .WithMessage(ClaimErrorMessages.ClaimDateOfDepartureCannotBeSmallerThanDateOfArrival);
    }
}
