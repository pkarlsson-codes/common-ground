using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.HeirTransfers.Events;

/// <summary>Raised when staff record who is taking the home and how they qualify. Recording again replaces the heir.</summary>
/// <param name="AggregateId">The heir transfer ID.</param>
/// <param name="GroundLeaseId">The lease the home is held on.</param>
/// <param name="FullName">The heir's full name.</param>
/// <param name="Email">The heir's email address.</param>
/// <param name="Phone">The heir's phone number, if given.</param>
/// <param name="Relationship">How the heir qualifies.</param>
/// <param name="MovedInOn">The day a household member moved into the home, for that relationship.</param>
/// <param name="VerifiedAnnualIncome">The heir's verified annual income, for any other heir.</param>
/// <param name="VerifiedHouseholdSize">The heir's household size, for any other heir.</param>
/// <param name="IncomeDocumentIds">The income documents, as Documents file IDs, for any other heir.</param>
/// <param name="IncomeLimit">The most the heir's household may earn, when the home has a limit; null when it has none.</param>
/// <param name="IncomeLimitPercentOfAmi">The home's limit as a percent of area median income, when it has one.</param>
/// <param name="IncomeLimitsEffectiveFrom">The day the schedule that set the limit took effect.</param>
/// <param name="RecordedBy">The staff user who recorded it.</param>
public sealed record HeirRecorded(
    string AggregateId,
    string GroundLeaseId,
    string FullName,
    string Email,
    string? Phone,
    HeirRelationship Relationship,
    DateOnly? MovedInOn,
    Money? VerifiedAnnualIncome,
    int? VerifiedHouseholdSize,
    IReadOnlyList<string> IncomeDocumentIds,
    Money? IncomeLimit,
    int? IncomeLimitPercentOfAmi,
    DateOnly? IncomeLimitsEffectiveFrom,
    string RecordedBy) : DomainEvent(AggregateId);
