using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.GroundLeases.Events;

/// <summary>
/// Raised when the home passes to an heir (Model Lease Art 10.3): the lease keeps its terms and ledger, and its
/// homeowner changes.
/// </summary>
/// <param name="TransferredOn">The day the home passed to the heir.</param>
/// <param name="PreviousFullName">The homeowner before the transfer, if the lease had one.</param>
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
