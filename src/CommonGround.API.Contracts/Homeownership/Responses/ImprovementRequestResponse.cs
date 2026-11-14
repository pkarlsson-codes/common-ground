namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>A homeowner's request to alter or add to the home.</summary>
/// <param name="Id">The request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="HomeId">The home the work is on.</param>
/// <param name="HomeownerFullName">The homeowner's full name when the request arrived.</param>
/// <param name="Status">Waiting for decision, Approved, Denied, Completed or Withdrawn.</param>
/// <param name="ReceivedOn">The day the request reached the trust.</param>
/// <param name="ReplyDueOn">The day the trust's reply is due: two weeks after receipt.</param>
/// <param name="Description">What the homeowner wants to do.</param>
/// <param name="EstimatedCost">The homeowner's cost estimate.</param>
/// <param name="ApprovedOn">The day the trust approved the work.</param>
/// <param name="PreApprovedCredit">The most the trust agreed to count.</param>
/// <param name="LeaseImprovementsCap">The lease's improvements cap when approved; null when the lease has none.</param>
/// <param name="CreditOnLeaseBefore">The confirmed credit the lease already had when this was approved.</param>
/// <param name="ApprovalNotes">What staff noted when approving.</param>
/// <param name="DeniedOn">The day the trust refused the work.</param>
/// <param name="DenialReason">Why the trust refused the work.</param>
/// <param name="CompletedOn">The day the work finished.</param>
/// <param name="ActualCost">What the work cost.</param>
/// <param name="ReceiptDocumentIds">The receipts, as Documents file IDs.</param>
/// <param name="ConfirmedCredit">The credit that counts toward the lease's improvements; null until completed.</param>
/// <param name="CompletionNotes">What staff noted when recording the finished work.</param>
/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
/// <param name="WithdrawnReason">Why the homeowner withdrew it.</param>
/// <param name="RecordedAt">When the request was recorded.</param>
public sealed record ImprovementRequestResponse(
    string Id,
    string GroundLeaseId,
    string HomeId,
    string? HomeownerFullName,
    string Status,
    DateOnly ReceivedOn,
    DateOnly ReplyDueOn,
    string Description,
    decimal EstimatedCost,
    DateOnly? ApprovedOn,
    decimal? PreApprovedCredit,
    decimal? LeaseImprovementsCap,
    decimal? CreditOnLeaseBefore,
    string? ApprovalNotes,
    DateOnly? DeniedOn,
    string? DenialReason,
    DateOnly? CompletedOn,
    decimal? ActualCost,
    IReadOnlyList<string> ReceiptDocumentIds,
    decimal? ConfirmedCredit,
    string? CompletionNotes,
    DateOnly? WithdrawnOn,
    string? WithdrawnReason,
    DateTimeOffset RecordedAt);

/// <summary>The ID of an improvement request just recorded.</summary>
/// <param name="ImprovementRequestId">The request ID.</param>
public sealed record ReceiveImprovementRequestResponse(string ImprovementRequestId);
