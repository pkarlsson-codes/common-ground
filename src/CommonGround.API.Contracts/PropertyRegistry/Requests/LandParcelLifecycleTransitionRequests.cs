namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to start parcel construction.</summary>
public sealed record StartParcelConstructionRequest;

/// <summary>Request to complete parcel construction.</summary>
public sealed record CompleteParcelConstructionRequest;

/// <summary>Request to suspend parcel construction.</summary>
public sealed record SuspendParcelConstructionRequest;

/// <summary>Request to list a parcel for placement.</summary>
public sealed record ListParcelForPlacementRequest;

/// <summary>Request to withdraw a parcel for redevelopment.</summary>
public sealed record WithdrawParcelForRedevelopmentRequest;

/// <summary>Request to decommission a parcel.</summary>
public sealed record DecommissionParcelRequest;

/// <summary>Request to recommission a parcel.</summary>
public sealed record RecommissionParcelRequest;