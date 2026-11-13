namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>
/// Request to open a foreclosure prevention case for a lease whose notice of default is unpaid. The case takes
/// today's date and the notice's figures from the lease's fee ledger.
/// </summary>
/// <param name="GroundLeaseId">The lease the case is for.</param>
public sealed record OpenForeclosurePreventionCaseForLeaseDefaultRequest(string GroundLeaseId);
