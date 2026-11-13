namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>A homeowner's request for a new loan on the home, with the debt check once the appraisal is in.</summary>
/// <param name="Id">The request ID.</param>
/// <param name="GroundLeaseId">The homeowner's lease.</param>
/// <param name="HomeId">The home the loan is secured on.</param>
/// <param name="HomeownerFullName">The homeowner's full name when the request arrived.</param>
/// <param name="Status">Pending, Approved, Denied, Closed or Withdrawn.</param>
/// <param name="ReceivedOn">The day the request reached the trust.</param>
/// <param name="Lender">The lender making the loan.</param>
/// <param name="Purpose">Refinance, SecondMortgage, HomeRepair or Other.</param>
/// <param name="Principal">The loan amount.</param>
/// <param name="TotalDebtAfterClosing">All mortgage debt on the home once the loan closes.</param>
/// <param name="InterestRatePercent">The yearly interest rate, in percent.</param>
/// <param name="RateType">Fixed or Adjustable.</param>
/// <param name="TermYears">The loan's term in years.</param>
/// <param name="ExpectedClosingOn">The day the loan is expected to close.</param>
/// <param name="BusinessDaysBeforeClosing">Business days (Monday to Friday) from receipt to the expected closing.</param>
/// <param name="ShortNotice">Whether that is fewer than the 15 business days the lease asks for.</param>
/// <param name="ClosingCosts">The loan's closing costs, if known.</param>
/// <param name="Notes">Anything staff noted.</param>
/// <param name="AppraisedValue">The loan's appraised value, once recorded.</param>
/// <param name="AppraisalDate">The day of the loan's appraisal.</param>
/// <param name="CurrentIndexValue">The index value the appraisal used, on an index-based lease.</param>
/// <param name="PurchaseOptionPrice">The purchase option price the appraisal gives.</param>
/// <param name="MaxMortgageDebtPercent">The lease's debt cap percent the check used.</param>
/// <param name="DebtCapAmount">The most total debt may be.</param>
/// <param name="DebtPercent">The total debt as a percent of the purchase option price, to one decimal place.</param>
/// <param name="OverCap">Whether the total debt is above the cap amount; null until the appraisal is in.</param>
/// <param name="DecidedOn">The day the trust approved or denied it.</param>
/// <param name="DecisionReason">Why the trust approved or denied it.</param>
/// <param name="ClosedOn">The day the loan closed.</param>
/// <param name="PermittedMortgageId">The permitted mortgage on the lease the closed loan became.</param>
/// <param name="PaidOffMortgageIds">The lease's mortgages the loan paid off.</param>
/// <param name="WithdrawnOn">The day the homeowner withdrew it.</param>
/// <param name="WithdrawnReason">Why the homeowner withdrew it.</param>
/// <param name="RecordedAt">When the request was recorded.</param>
public sealed record MortgageRequestResponse(
    string Id,
    string GroundLeaseId,
    string HomeId,
    string? HomeownerFullName,
    string Status,
    DateOnly ReceivedOn,
    string Lender,
    string Purpose,
    decimal Principal,
    decimal TotalDebtAfterClosing,
    decimal InterestRatePercent,
    string RateType,
    int TermYears,
    DateOnly ExpectedClosingOn,
    int BusinessDaysBeforeClosing,
    bool ShortNotice,
    decimal? ClosingCosts,
    string? Notes,
    decimal? AppraisedValue,
    DateOnly? AppraisalDate,
    decimal? CurrentIndexValue,
    decimal? PurchaseOptionPrice,
    int? MaxMortgageDebtPercent,
    decimal? DebtCapAmount,
    decimal? DebtPercent,
    bool? OverCap,
    DateOnly? DecidedOn,
    string? DecisionReason,
    DateOnly? ClosedOn,
    string? PermittedMortgageId,
    IReadOnlyList<string> PaidOffMortgageIds,
    DateOnly? WithdrawnOn,
    string? WithdrawnReason,
    DateTimeOffset RecordedAt);

/// <summary>The ID of a mortgage request just recorded.</summary>
/// <param name="MortgageRequestId">The request ID.</param>
public sealed record ReceiveMortgageRequestResponse(string MortgageRequestId);

/// <summary>The permitted mortgage a closed loan became.</summary>
/// <param name="PermittedMortgageId">The new permitted mortgage's ID on the lease.</param>
public sealed record RecordMortgageLoanClosedResponse(string PermittedMortgageId);
