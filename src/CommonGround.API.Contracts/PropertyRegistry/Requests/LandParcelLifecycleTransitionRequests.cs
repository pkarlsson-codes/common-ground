namespace CommonGround.API.Contracts.PropertyRegistry.Requests;

/// <summary>Request to start parcel predevelopment.</summary>
public sealed record StartParcelPredevelopmentRequest;

/// <summary>Request to start parcel construction.</summary>
public sealed record StartParcelConstructionRequest;

/// <summary>Request to mark a parcel ready for occupancy.</summary>
public sealed record MarkParcelReadyForOccupancyRequest;

/// <summary>Request to decommission a parcel.</summary>
public sealed record DecommissionParcelRequest;

/// <summary>Request to recommission a parcel.</summary>
public sealed record RecommissionParcelRequest;

/// <summary>Request to mark a parcel occupied.</summary>
public sealed record MarkParcelOccupiedRequest;

/// <summary>Request to enter parcel transitioning.</summary>
public sealed record EnterParcelTransitioningRequest;

/// <summary>Request to return a parcel to available.</summary>
public sealed record ReturnParcelToAvailableRequest;