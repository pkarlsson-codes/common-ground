using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;

/// <summary>
/// The household's income, size or accessibility need changed, and its open application took the new figures.
/// <paramref name="QualificationToConfirm"/> is the application's mark after the change: set when an approved
/// application's income or size now differs from the figures it was last qualified on, null when it doesn't or the
/// application isn't approved yet.
/// </summary>
/// <param name="AggregateId">The application.</param>
/// <param name="HouseholdId">The household that changed.</param>
/// <param name="HouseholdSize">The household's size now.</param>
/// <param name="AnnualIncome">The household's annual income now.</param>
/// <param name="AccessibilityRequired">Whether the household needs an accessible home now.</param>
/// <param name="ChangedDate">The day the application took the change, in the trust's time zone.</param>
/// <param name="QualificationToConfirm">The mark after the change, or null when there is none.</param>
public sealed record ApplicationDeclarationUpdated(
    string AggregateId,
    string HouseholdId,
    int HouseholdSize,
    Money AnnualIncome,
    bool AccessibilityRequired,
    DateOnly ChangedDate,
    QualificationToConfirmMark? QualificationToConfirm) : DomainEvent(AggregateId);

/// <summary>
/// An approved application whose income or size changed after staff last checked it still qualifies.
/// </summary>
/// <param name="Since">The day of the first change not yet confirmed.</param>
/// <param name="QualifiedAnnualIncome">The income staff last checked: at approval, or at the last confirmation.</param>
/// <param name="QualifiedHouseholdSize">The household size staff last checked.</param>
public sealed record QualificationToConfirmMark(
    DateOnly Since,
    Money QualifiedAnnualIncome,
    int QualifiedHouseholdSize);
