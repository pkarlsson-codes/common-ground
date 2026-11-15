using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.Homeownership.Domain.Resales.Events;

/// <summary>
/// Raised when the appraisal the lease requires after a notice to sell is recorded. A later one replaces it.
/// The prices are worked out when it is recorded and kept, so a later change to the lease can't change them.
/// </summary>
/// <param name="AggregateId">The resale's ID.</param>
/// <param name="AppraisedValue">The home's appraised market value, without the lease's restrictions.</param>
/// <param name="AppraisalDate">The day of the appraisal.</param>
/// <param name="AppraiserName">Who appraised the home.</param>
/// <param name="ApprovedImprovements">The trust-approved improvements credited in the formula, if the lease's formula credits them.</param>
/// <param name="CurrentIndexValue">The current index value, when the lease's formula is index based.</param>
/// <param name="FormulaPrice">The most the lease's formula allows the home to sell for.</param>
/// <param name="PurchaseOptionPrice">What the trust pays to buy the home: the lesser of the formula price and the appraised value.</param>
/// <param name="RecordedBy">The user who recorded the appraisal.</param>
public sealed record ResaleAppraisalRecorded(
    string AggregateId,
    Money AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    Money? ApprovedImprovements,
    decimal? CurrentIndexValue,
    Money FormulaPrice,
    Money PurchaseOptionPrice,
    string RecordedBy) : DomainEvent(AggregateId);
