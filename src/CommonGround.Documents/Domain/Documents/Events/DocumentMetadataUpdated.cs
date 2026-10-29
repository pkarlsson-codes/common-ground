using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Documents.Domain.Documents.Events;

/// <summary>Raised when a Document's editable metadata changes (title, sensitivity, expiry).</summary>
public sealed record DocumentMetadataUpdated(
    string AggregateId,
    string Title,
    string Sensitivity,
    DateOnly? ExpiresOn,
    bool ClearExpiresOn,
    string UpdatedBy) : DomainEvent(AggregateId);
