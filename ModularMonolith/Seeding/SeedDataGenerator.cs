using Modules.Claims.Domain.Entities;
using Modules.Claims.Domain.Enums;

namespace ModularMonolith.Seeding;

internal sealed record SeededLookups(
    List<Reason> Reasons,
    List<Solution> Solutions,
    List<FollowedBy> FollowedBies,
    List<RefundState> RefundStates,
    List<CompensationReason> CompensationReasons,
    List<SalesChannel> SalesChannels,
    List<Service> Services,
    List<SkissimType> SkissimTypes);

internal static class SeedDataGenerator
{
    private static readonly string[] FirstNames =
    [
        "Emma", "Liam", "Olivia", "Noah", "Ava", "Lucas", "Mia", "Hugo", "Chloe", "Leo",
        "Camille", "Louis", "Sarah", "Jules", "Manon", "Nathan", "Lea", "Adam", "Ines", "Paul"
    ];

    private static readonly string[] LastNames =
    [
        "Martin", "Bernard", "Dubois", "Thomas", "Robert", "Petit", "Durand", "Leroy",
        "Moreau", "Simon", "Laurent", "Lefebvre", "Michel", "Garcia", "David", "Bertrand"
    ];

    private static readonly string[] Products = ["Ski Package", "Chalet Stay", "Resort Combo", "Family Deal", "Group Trip", "Weekend Getaway"];

    private static readonly (string Label, string Value)[] ReasonPool =
    [
        ("Accueil sur place", "AccueilSurPlace"),
        ("Descriptif", "Descriptif"),
        ("Réception documents", "ReceptionDocuments"),
        ("Logement non conforme", "LogementNonConforme"),
        ("Manque prestations", "ManquePrestations"),
        ("Prestations non consommées", "PrestationsNonConsommees"),
        ("Ménage", "Menage"),
        ("Vétusté", "Vetuste"),
        ("Ménage et vétusté", "MenageEtVetuste"),
        ("Note de confort", "NoteDeConfort"),
        ("Nuisances", "Nuisances"),
        ("Pb techniques", "PbTechniques"),
        ("Pb multiples", "PbMultiples")
    ];

    private static readonly (string Label, string Value)[] FollowedByPool =
    [
        ("Aurore", "Aurore"),
        ("Jennifer", "Jennifer"),
        ("Lindy", "Lindy"),
        ("Angela", "Angela"),
        ("Gwendoline", "Gwendoline"),
        ("Magali", "Magali"),
        ("Elisa", "Elisa"),
        ("Guillaume", "Guillaume"),
        ("Manoel", "Manoel"),
        ("Romain", "Romain"),
        ("Amélie", "Amelie"),
        ("Romane", "Romane")
    ];

    private static readonly (string Label, string Value)[] SolutionPool =
    [
        ("Remboursement", "Remboursement"),
        ("Avoir", "Avoir"),
        ("Remboursement et avoir", "RemboursementEtAvoir"),
        ("Refus de la demande", "RefusDeLaDemande")
    ];

    private static readonly (string Label, string Value)[] RefundStatePool =
    [
        ("Oui", "Oui"),
        ("Non", "Non"),
        ("En attente RIB", "EnAttenteRib"),
        ("En attente STRIPE", "EnAttenteStripe")
    ];

    private static readonly (string Label, string Value)[] CompensationReasonPool =
    [
        ("Erreur Fournisseur", "ErreurFournisseur"),
        ("Erreur TF", "ErreurTf"),
        ("Erreur Frn et TF", "ErreurFrnEtTf"),
        ("Dédommagement sur place", "DedommagementSurPlace"),
        ("Réclamation non valable", "ReclamationNonValable")
    ];

    private static readonly (string Label, string Value, string Language)[] SalesChannelPool =
    [
        ("tfski.com", "TFskiCom", "Fr"),
        ("tfski.co.uk", "TFskiCoUk", "En"),
        ("tfski.nl", "TFskiNl", "Nl"),
        ("Lastminute", "Lastminute", "Fr"),
        ("Airbnb", "Airbnb", "Fr"),
        ("booking.com", "BookingCom", "Fr"),
        ("C.G.O.S", "Cgos", "Fr"),
        ("Carrefour voyages", "CarrefourVoyages", "Fr"),
        ("E.LECLERC", "ELeclerc", "Fr"),
        ("VEEPEE", "Veepee", "Fr"),
        ("expedia", "Expedia", "Fr"),
        ("LIDL VOYAGES", "LidlVoyages", "Fr"),
        ("MEYCLUB", "Meyclub", "Fr"),
        ("Showroomprivee", "Showroomprivee", "Fr"),
        ("WIISMILE VOYAGES", "WiismileVoyages", "Fr")
    ];

    private static readonly (string Label, string Value)[] ServicePool =
    [
        ("Hebergement", "Hebergement"),
        ("Skipass", "Skipass"),
        ("Location de ski", "LocationDeSki"),
        ("ESF", "Esf"),
        ("Foodpack", "Foodpack"),
        ("Skitrain UK", "SkitrainUk"),
        ("Transfert", "Transfert"),
        ("Assurance", "Assurance"),
        ("Parking", "Parking"),
        ("Activites", "Activites")
    ];

    // ServiceIndex lines up 1:1 with ServicePool's order above (0=Hebergement ... 9=Activites).
    private static readonly (string Label, int ServiceIndex)[] SupplierPool =
    [
        ("ACTION PLEINE NATURE", 0), ("ACTISOURCE - LA BOUTIQUE", 0), ("ADELIE IMMOBILIER", 0), ("AGENCE CARROZ IMMOBILIER", 0), ("AGENCE CRUZ", 0),
        ("ADS", 1), ("ALTISERVICE - FONT ROMEU", 1), ("ALTISERVICE SAINT LARY", 1), ("CHATEL RESERVATION", 1), ("COLOMION S.P.A", 1),
        ("Intersport", 2), ("Skiset", 2), ("Sport 2000", 2), ("Precision Ski", 2), ("Go Sport", 2),
        ("ESF - ALPE D'HUEZ", 3), ("ESF - AUSSOIS", 3), ("ESF - AVORIAZ", 3), ("ESF - CHÂTEL", 3), ("ESF - COURCHEVEL 1850", 3),
        ("OFFCOURSES", 4),
        ("Eurostar", 5),
        ("TRANSDEV", 6),
        ("ASSURINCO", 7), 
        ("ALTIPARCS", 8), ("VALTHOPARC", 8), ("VALMOREL GESTION PARKING", 8), ("MERIBEL PARKING", 8), ("SAGS", 8),
        ("CHATEL RESERVATION", 9), ("OFFICE DU TOURISME DES ARCS", 9), ("SOGEVAB", 9), ("SEMLORE", 9), ("RM / OT", 9)
    ];

    private static readonly (string Label, string Value)[] SkissimTypePool =
    [
        ("None", "None"),
        ("Select", "Select"),
        ("Premium", "Premium"),
        ("Classic", "Classic")
    ];

    private static readonly (string Label, string Value)[] ClaimSummaryPool =
    [
        ("Awaiting supplier confirmation on refund eligibility", "AwaitingSupplierConfirmation"),
        ("Customer has been offered a partial voucher as compensation", "PartialVoucherOffered"),
        ("Escalated to supplier for further investigation", "EscalatedToSupplier"),
        ("Resolved after supplier agreed to reimburse the customer", "ResolvedSupplierReimbursed")
    ];

    private static readonly string?[] PurposeOfSolutionPool =
    [
        "Goodwill gesture to retain customer loyalty.",
        "Supplier failed to honor the original booking terms.",
        "Compensation for the inconvenience caused during the trip.",
        null
    ];

    internal static SeededLookups GenerateLookups() => new(
        Reasons: [.. ReasonPool.Select(x => new Reason { Id = Guid.CreateVersion7(), Label = x.Label, Value = x.Value })],
        Solutions: [.. SolutionPool.Select(x => new Solution { Id = Guid.CreateVersion7(), Label = x.Label, Value = x.Value })],
        FollowedBies: [.. FollowedByPool.Select(x => new FollowedBy { Id = Guid.CreateVersion7(), Label = x.Label, Value = x.Value })],
        RefundStates: [.. RefundStatePool.Select(x => new RefundState { Id = Guid.CreateVersion7(), Label = x.Label, Value = x.Value })],
        CompensationReasons: [.. CompensationReasonPool.Select(x => new CompensationReason { Id = Guid.CreateVersion7(), Label = x.Label, Value = x.Value })],
        SalesChannels: [.. SalesChannelPool.Select(x => new SalesChannel { Id = Guid.CreateVersion7(), Label = x.Label, Value = x.Value, Language = x.Language })],
        Services: [.. ServicePool.Select(x => new Service { Id = Guid.CreateVersion7(), Label = x.Label, Value = x.Value })],
        SkissimTypes: [.. SkissimTypePool.Select(x => new SkissimType { Id = Guid.CreateVersion7(), Label = x.Label, Value = x.Value })]);

    internal static List<Supplier> GenerateSuppliers(IReadOnlyList<Service> services, Random random) =>
        SupplierPool.Select(x =>
        {
            var service = services[x.ServiceIndex];
            return new Supplier
            {
                Id = Guid.CreateVersion7(),
                Label = x.Label,
                Value = x.Label,
                SupplierAkioNumber = random.Next(10000, 99999),
                ServiceId = service.Id,
                Service = service
            };
        }).ToList();

    internal static Claim GenerateClaim(Supplier supplier, SeededLookups lookups, Random random)
    {
        var claimId = Guid.CreateVersion7();
        var bookingId = Guid.CreateVersion7();
        var reason = PickRandom(lookups.Reasons, random);
        var solution = PickRandom(lookups.Solutions, random);
        var followedBy = random.Next(2) == 0 ? PickRandom(lookups.FollowedBies, random) : null;
        var claimSummary = PickRandom(ClaimSummaryPool, random);

        return new Claim
        {
            Id = claimId,
            State = RandomEnum<ClaimState>(random),
            FollowedById = followedBy?.Id,
            FollowedBy = followedBy,
            ReasonId = reason.Id,
            Reason = reason,
            ClaimSummary = claimSummary.Label,
            SolutionId = solution.Id,
            Solution = solution,
            PurposeOfSolution = PickRandom(PurposeOfSolutionPool, random),
            UpdateReason = null,
            CustomerSuppInfo = random.Next(3) == 0 ? "Customer provided additional documentation." : null,
            SupplierSuppInfo = random.Next(3) == 0 ? "Supplier acknowledged receipt of the claim." : null,
            BookingId = bookingId,
            Booking = GenerateBooking(bookingId, supplier, lookups, random),
            ClaimDate = GenerateClaimDate(claimId, random),
            Compensation = GenerateCompensation(claimId, lookups, random)
        };
    }

    private static Booking GenerateBooking(Guid bookingId, Supplier supplier, SeededLookups lookups, Random random)
    {
        var customerId = Guid.CreateVersion7();
        var salesChannel = PickRandom(lookups.SalesChannels, random);
        var skissimType = PickRandom(lookups.SkissimTypes, random);

        return new Booking
        {
            Id = bookingId,
            BookingNumber = $"BK{random.Next(100000, 999999)}",
            SalesChannelId = salesChannel.Id,
            SalesChannel = salesChannel,
            Language = RandomEnum<Language>(random),
            SkissimTypeId = skissimType.Id,
            SkissimType = skissimType,
            Product = PickRandom(Products, random),
            CustomerId = customerId,
            Customer = new Customer
            {
                Id = customerId,
                Name = RandomFullName(random),
                AkioNumber = random.Next(100000, 999999)
            },
            SupplierId = supplier.Id,
            Supplier = supplier
        };
    }

    private static ClaimDate GenerateClaimDate(Guid claimId, Random random)
    {
        var dateOfReceivedClaim = DateTimeOffset.UtcNow.AddDays(-random.Next(0, 730));
        var dateOfArrival = DateTimeOffset.UtcNow.AddDays(random.Next(-365, 365));
        var dateOfDeparture = dateOfArrival.AddDays(random.Next(1, 21));

        return new ClaimDate
        {
            Id = Guid.CreateVersion7(),
            ClaimId = claimId,
            DateOfReceivedClaim = dateOfReceivedClaim,
            DateOfStartFollowUp = dateOfReceivedClaim.AddDays(random.Next(0, 5)),
            DateLastUpdate = dateOfReceivedClaim.AddDays(random.Next(0, 30)),
            DateOfDeparture = dateOfDeparture,
            DateEndOfFollowUp = random.Next(2) == 0 ? dateOfReceivedClaim.AddDays(random.Next(5, 60)) : null,
            DateOfArrival = dateOfArrival
        };
    }

    private static Compensation GenerateCompensation(Guid claimId, SeededLookups lookups, Random random)
    {
        var refundState = PickRandom(lookups.RefundStates, random);
        var compensationReason = PickRandom(lookups.CompensationReasons, random);
        return new()
        {
            Id = Guid.CreateVersion7(),
            ClaimId = claimId,
            CustomerVoucher = random.Next(2) == 0 ? (float)random.Next(0, 500) : null,
            CustomerUsedVoucher = random.Next(2) == 0 ? (float)random.Next(0, 500) : null,
            SupplierRefund = random.Next(2) == 0 ? (float)random.Next(0, 1000) : null,
            ClaimRefund = random.Next(2) == 0 ? (float)random.Next(0, 1000) : null,
            RefundStateId = refundState.Id,
            RefundState = refundState,
            CompensationReasonId = compensationReason.Id,
            CompensationReason = compensationReason
        };
    }

    private static string RandomFullName(Random random) => $"{PickRandom(FirstNames, random)} {PickRandom(LastNames, random)}";

    private static T PickRandom<T>(IReadOnlyList<T> pool, Random random) => pool[random.Next(pool.Count)];

    private static TEnum RandomEnum<TEnum>(Random random) where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        return values[random.Next(values.Length)];
    }
}
