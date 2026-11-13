using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a mortgage the trust permitted outside CommonGround on a lease.</summary>
/// <param name="Lender">The lender or servicer.</param>
/// <param name="NoticeAddress">Where the trust sends copies of notices, or null when not known.</param>
/// <param name="OriginalPrincipal">The loan amount when it closed.</param>
/// <param name="PermittedOn">The day the trust permitted it; not in the future, not before the lease start.</param>
public sealed record RecordPermittedMortgageRequest(
    string Lender,
    [property: JsonRequired] string? NoticeAddress,
    [property: JsonRequired] decimal OriginalPrincipal,
    DateOnly PermittedOn);

/// <summary>Request to change a permitted mortgage's lender and notice address.</summary>
/// <param name="Lender">The lender or servicer now.</param>
/// <param name="NoticeAddress">Where the trust sends copies of notices now, or null when not known.</param>
public sealed record EditPermittedMortgageRequest(
    string Lender,
    [property: JsonRequired] string? NoticeAddress);

/// <summary>Request to mark a permitted mortgage paid off.</summary>
/// <param name="PaidOffOn">The day it was paid off; not in the future, not before it was permitted.</param>
public sealed record MarkPermittedMortgagePaidOffRequest(DateOnly PaidOffOn);
