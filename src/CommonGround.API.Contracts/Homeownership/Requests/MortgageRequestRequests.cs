using System.Text.Json.Serialization;

namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record a homeowner's request for a new loan on the home (Model Lease Art 8.3).</summary>
/// <param name="ReceivedOn">The day the request reached the trust; not in the future, not before the lease start.</param>
/// <param name="Lender">The lender making the loan.</param>
/// <param name="Purpose">Refinance, SecondMortgage, HomeRepair or Other.</param>
/// <param name="Principal">The loan amount.</param>
/// <param name="TotalDebtAfterClosing">All mortgage debt on the home once the loan closes, the new loan included.</param>
/// <param name="InterestRatePercent">The yearly interest rate, in percent.</param>
/// <param name="RateType">Fixed or Adjustable.</param>
/// <param name="TermYears">The loan's term in years, 1 to 50.</param>
/// <param name="ExpectedClosingOn">The day the loan is expected to close; not before the received date.</param>
/// <param name="ClosingCosts">The loan's closing costs, or null when not yet known.</param>
/// <param name="AppraisedValue">The loan's appraised value, or null when the appraisal hasn't come; sent with <paramref name="AppraisalDate"/>.</param>
/// <param name="AppraisalDate">The day of the loan's appraisal, or null when it hasn't come.</param>
/// <param name="CurrentIndexValue">The index value on the appraisal date, when the lease's formula is index based; otherwise null.</param>
/// <param name="Notes">Anything staff want to note, or null.</param>
public sealed record ReceiveMortgageRequestRequest(
    DateOnly ReceivedOn,
    string Lender,
    string Purpose,
    [property: JsonRequired] decimal Principal,
    [property: JsonRequired] decimal TotalDebtAfterClosing,
    [property: JsonRequired] decimal InterestRatePercent,
    string RateType,
    [property: JsonRequired] int TermYears,
    DateOnly ExpectedClosingOn,
    [property: JsonRequired] decimal? ClosingCosts,
    [property: JsonRequired] decimal? AppraisedValue,
    [property: JsonRequired] DateOnly? AppraisalDate,
    [property: JsonRequired] decimal? CurrentIndexValue,
    [property: JsonRequired] string? Notes);

/// <summary>Request to record the loan's appraisal and closing costs on a request waiting for a decision.</summary>
/// <param name="AppraisedValue">The home's appraised value for the loan.</param>
/// <param name="AppraisalDate">The day of the appraisal; not in the future.</param>
/// <param name="CurrentIndexValue">The index value on the appraisal date, when the lease's formula is index based; otherwise null.</param>
/// <param name="ClosingCosts">The loan's closing costs, or null when not yet known.</param>
public sealed record RecordMortgageRequestAppraisalRequest(
    [property: JsonRequired] decimal AppraisedValue,
    DateOnly AppraisalDate,
    [property: JsonRequired] decimal? CurrentIndexValue,
    [property: JsonRequired] decimal? ClosingCosts);

/// <summary>Request to give the trust's written permission for a requested loan.</summary>
/// <param name="ApprovedOn">The day the trust approved it.</param>
/// <param name="Reason">Why the trust approved it; required when the debt is over the cap, otherwise may be null.</param>
public sealed record ApproveMortgageRequestRequest(
    DateOnly ApprovedOn,
    [property: JsonRequired] string? Reason);

/// <summary>Request to refuse a requested loan.</summary>
/// <param name="DeniedOn">The day the trust refused it.</param>
/// <param name="Reason">Why, for the denial letter.</param>
public sealed record DenyMortgageRequestRequest(DateOnly DeniedOn, string Reason);

/// <summary>Request to record that an approved loan closed.</summary>
/// <param name="ClosedOn">The day the loan closed.</param>
/// <param name="PaidOffMortgageIds">The lease's permitted mortgages the loan paid off; empty when none.</param>
/// <param name="NoticeAddress">Where the lender takes notices about the home, or null when not known yet.</param>
public sealed record RecordMortgageLoanClosedRequest(
    DateOnly ClosedOn,
    [property: JsonRequired] IReadOnlyList<string> PaidOffMortgageIds,
    [property: JsonRequired] string? NoticeAddress);

/// <summary>Request to record that the homeowner dropped a requested loan before it closed.</summary>
/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
/// <param name="Reason">Why the homeowner withdrew it.</param>
public sealed record WithdrawMortgageRequestRequest(DateOnly WithdrawnOn, string Reason);
