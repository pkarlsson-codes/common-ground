using CommonGround.API.Contracts.Stewardship.Responses;

namespace CommonGround.Management.Gateway.Models;

/// <summary>An open stewardship case with the homeowner and address of its ground lease.</summary>
public sealed record StewardshipQueueItemResponse
{
    /// <summary>Gets the stewardship case.</summary>
    required public StewardshipCaseResponse StewardshipCase { get; init; }

    /// <summary>Gets the lease's homeowner, or null if none is onboarded.</summary>
    public string? HomeownerFullName { get; init; }

    /// <summary>Gets the address of the lease's land parcel, or null if unknown.</summary>
    public string? Address { get; init; }
}

/// <summary>An unresolved foreclosure prevention case with the homeowner and address of its ground lease.</summary>
public sealed record ForeclosurePreventionQueueItemResponse
{
    /// <summary>Gets the foreclosure prevention case.</summary>
    required public ForeclosurePreventionCaseResponse ForeclosurePreventionCase { get; init; }

    /// <summary>Gets the lease's homeowner, or null if none is onboarded.</summary>
    public string? HomeownerFullName { get; init; }

    /// <summary>Gets the address of the lease's land parcel, or null if unknown.</summary>
    public string? Address { get; init; }
}
