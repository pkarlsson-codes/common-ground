using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.ImprovementRequests.Events;

/// <summary>Raised when the trust receives a homeowner's request to alter or add to the home (Model Lease Art 7.3).</summary>
/// <param name="AggregateId">The improvement request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="HomeId">The home the work is on.</param>
/// <param name="HomeownerFullName">The homeowner's full name when the request arrived.</param>
/// <param name="ReceivedOn">The day the request reached the trust.</param>
/// <param name="Description">What the homeowner wants to do.</param>
/// <param name="EstimatedCost">The homeowner's estimate of the work's cost.</param>
/// <param name="ReceivedBy">The staff user who recorded it.</param>
public sealed record ImprovementRequestReceived(
    string AggregateId,
    string GroundLeaseId,
    string HomeId,
    string? HomeownerFullName,
    DateOnly ReceivedOn,
    string Description,
    Money EstimatedCost,
    string ReceivedBy) : DomainEvent(AggregateId);
