namespace Modules.Claims.Features.Features.Shared.Responses;

public sealed record LookupsResponse(
    IReadOnlyList<ReasonResponse> Reasons,
    IReadOnlyList<SolutionResponse> Solutions,
    IReadOnlyList<FollowedByResponse> FollowedBies,
    IReadOnlyList<RefundStateResponse> RefundStates,
    IReadOnlyList<CompensationReasonResponse> CompensationReasons,
    IReadOnlyList<SalesChannelResponse> SalesChannels,
    IReadOnlyList<ServiceResponse> Services,
    IReadOnlyList<SupplierResponse> Suppliers,
    IReadOnlyList<SkissimTypeResponse> SkissimTypes);
