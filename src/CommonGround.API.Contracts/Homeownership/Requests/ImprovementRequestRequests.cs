using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a homeowner's request to alter or add to the home (Model Lease Art 7.3).</summary>
/// <param name="ReceivedOn">The day the request reached the trust; not in the future, not before the lease start.</param>
/// <param name="Description">What the homeowner wants to do.</param>
/// <param name="EstimatedCost">The homeowner's cost estimate; above zero.</param>
public sealed record ReceiveImprovementRequestRequest(
    DateOnly ReceivedOn,
    string Description,
    [property: JsonRequired] decimal EstimatedCost);

/// <summary>Request to approve the work with a pre-approved credit.</summary>
/// <param name="ApprovedOn">The day the trust approved it; not before receipt, not in the future.</param>
/// <param name="PreApprovedCredit">The most the trust will count, from zero to the estimated cost.</param>
/// <param name="Notes">Anything staff want to note, or null.</param>
public sealed record ApproveImprovementRequestRequest(
    DateOnly ApprovedOn,
    [property: JsonRequired] decimal PreApprovedCredit,
    [property: JsonRequired] string? Notes);

/// <summary>Request to refuse the work.</summary>
/// <param name="DeniedOn">The day the trust refused it.</param>
/// <param name="Reason">Why, for the denial letter.</param>
public sealed record DenyImprovementRequestRequest(DateOnly DeniedOn, string Reason);

/// <summary>Request to record the finished work and confirm the credit.</summary>
/// <param name="CompletedOn">The day the work finished; not before approval, not in the future.</param>
/// <param name="ActualCost">What the work cost; above zero.</param>
/// <param name="ReceiptDocumentIds">The receipts, as Documents file IDs. At least one.</param>
/// <param name="ConfirmedCredit">The credit that counts, from zero to the pre-approved credit.</param>
/// <param name="Notes">Anything staff want to note, or null.</param>
public sealed record RecordImprovementCompletedRequest(
    DateOnly CompletedOn,
    [property: JsonRequired] decimal ActualCost,
    [property: JsonRequired] IReadOnlyList<string> ReceiptDocumentIds,
    [property: JsonRequired] decimal ConfirmedCredit,
    [property: JsonRequired] string? Notes);

/// <summary>Request to record that the homeowner dropped the work.</summary>
/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
/// <param name="Reason">Why the homeowner withdrew it.</param>
public sealed record WithdrawImprovementRequestRequest(DateOnly WithdrawnOn, string Reason);
