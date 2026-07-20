using Bogus;
using Modules.Claims.Domain.Enums;
using Modules.Claims.Features.Features.Shared.Requests;

namespace Modules.Claims.Features.Integration.Tests.Shared;

internal static class ClaimRequestFactory
{
    internal static ClaimRequest CreateValid(DateTimeOffset? dateOfReceivedClaim = null)
    {
        var faker = new Faker();

        return new ClaimRequest(
            State: faker.PickRandom<ClaimState>(),
            FollowedBy: faker.Person.FullName,
            Reason: faker.Lorem.Sentence(),
            ClaimSummary: faker.Lorem.Paragraph(),
            Solution: faker.PickRandom<ClaimSolution>(),
            PurposeOfSolution: faker.Lorem.Sentence(),
            UpdateReason: null,
            CustomerSuppInfo: faker.Lorem.Sentence(),
            SupplierSuppInfo: faker.Lorem.Sentence(),
            Booking: new BookingRequest(
                BookingNumber: faker.Random.Replace("BK####"),
                SalesChannel: faker.PickRandom<SalesChannel>(),
                Language: faker.PickRandom<Language>(),
                SeasonLabel: faker.Commerce.Department(),
                SeasonValue: faker.Random.Word(),
                Service: faker.PickRandom<BookingService>(),
                Skissim: faker.Random.Bool(),
                SkissimType: faker.PickRandom<SkissimType>(),
                Product: faker.Commerce.ProductName(),
                Customer: new CustomerRequest(
                    Name: faker.Person.FullName,
                    AkioNumber: faker.Random.Int(1000, 9999)),
                Supplier: new SupplierRequest(
                    Name: faker.Company.CompanyName(),
                    SupplierAkioNumber: faker.Random.Int(1000, 9999))),
            ClaimDate: new ClaimDateRequest(
                DateOfReceivedClaim: dateOfReceivedClaim ?? DateTimeOffset.UtcNow.AddDays(-faker.Random.Int(0, 30)),
                DateOfStartFollowUp: null,
                DateLastUpdate: null,
                DateOfDeparture: null,
                DateEndOfFollowUp: null,
                DateOfArrival: null),
            Compensation: new CompensationRequest(
                CustomerVoucher: faker.Random.Float(0, 500),
                CustomerUsedVoucher: null,
                SupplierRefund: null,
                ClaimRefund: null,
                RefundState: faker.PickRandom<RefundState>()));
    }

    internal static ClaimRequest WithEmptyBookingNumber(ClaimRequest request) =>
        request with { Booking = request.Booking with { BookingNumber = string.Empty } };

    internal static ClaimRequest WithEmptyCustomerName(ClaimRequest request) =>
        request with
        {
            Booking = request.Booking with
            {
                Customer = request.Booking.Customer with { Name = string.Empty }
            }
        };

    internal static ClaimRequest WithEmptySupplierName(ClaimRequest request) =>
        request with
        {
            Booking = request.Booking with
            {
                Supplier = request.Booking.Supplier with { Name = string.Empty }
            }
        };

    internal static ClaimRequest WithNullBooking(ClaimRequest request) =>
        request with { Booking = null! };

    internal static ClaimRequest WithNullCustomer(ClaimRequest request) =>
        request with { Booking = request.Booking with { Customer = null! } };

    internal static ClaimRequest WithNullSupplier(ClaimRequest request) =>
        request with { Booking = request.Booking with { Supplier = null! } };

    internal static ClaimRequest WithDepartureAfterArrival(ClaimRequest request) =>
        request with
        {
            ClaimDate = request.ClaimDate with
            {
                DateOfDeparture = DateTimeOffset.UtcNow,
                DateOfArrival = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };
}
