using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the trust receives a homeowner's request to alter or add to the home (Model Lease Art 7.3).</summary>
/// <param name="HomeownerFullName">The homeowner's full name when the request arrived.</param>
/// <param name="ReceivedOn">The day the request reached the trust.</param>
public sealed record ImprovementRequestReceived(
    string AggregateId,
    string GroundLeaseId,
    string HomeId,
    string? HomeownerFullName,
    DateOnly ReceivedOn,
    string Description,
    Money EstimatedCost,
    string ReceivedBy) : DomainEvent(AggregateId);
