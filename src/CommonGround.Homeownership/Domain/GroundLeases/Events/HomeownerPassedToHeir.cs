using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>
/// Raised when the home passes to an heir (Model Lease Art 10.3): the lease keeps its terms and ledger, and its
/// homeowner changes.
/// </summary>
/// <param name="AggregateId">The ground lease's ID.</param>
/// <param name="HeirTransferId">The heir transfer that moved the home.</param>
/// <param name="TransferredOn">The day the home passed to the heir.</param>
/// <param name="Relationship">How the heir qualified.</param>
/// <param name="PreviousFullName">The homeowner before the transfer, if the lease had one.</param>
/// <param name="PreviousEmail">The previous homeowner's email.</param>
/// <param name="PreviousPhone">The previous homeowner's phone number.</param>
/// <param name="HomeownerFullName">The new homeowner's full name.</param>
/// <param name="HomeownerEmail">The new homeowner's email.</param>
/// <param name="HomeownerPhone">The new homeowner's phone number.</param>
/// <param name="RecordedBy">The staff user who completed the transfer.</param>
public sealed record HomeownerPassedToHeir(
    string AggregateId,
    string HeirTransferId,
    DateOnly TransferredOn,
    string Relationship,
    string? PreviousFullName,
    string? PreviousEmail,
    string? PreviousPhone,
    string HomeownerFullName,
    string HomeownerEmail,
    string? HomeownerPhone,
    string RecordedBy) : DomainEvent(AggregateId);
