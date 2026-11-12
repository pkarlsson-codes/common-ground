namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>
/// Detail response for a purchase workflow. <c>DeclaredAnnualIncome</c> and <c>DeclaredHouseholdSize</c> are what the
/// household declared when selected, and <c>IncomeLimit</c> the limit the home's selection judged by; all null when the
/// purchase didn't start from a selection, and <c>IncomeLimit</c> null when the home had no limit.
/// </summary>
public sealed record PurchaseWorkflowResponse(
    string Id,
    string GroundLeaseTemplateId,
    string LandParcelId,
    string HomeId,
    string? HomeLabel,
    string? ApplicationId,
    string? HouseholdId,
    string Phase,
    string InitiatedBy,
    string BuyerFullName,
    string BuyerEmail,
    string? BuyerPhone,
    decimal PurchasePrice,
    DateTimeOffset CreatedAt,
    decimal? DeclaredAnnualIncome,
    int? DeclaredHouseholdSize,
    PurchaseIncomeLimitDto? IncomeLimit,
    EligibilityDetailsDto? EligibilityDetails,
    AppraisalDetailsDto? AppraisalDetails,
    FinancingDetailsDto? FinancingDetails,
    InspectionResultDto? InspectionResult,
    TitleDetailsDto? TitleDetails,
    LeaseExecutionDetailsDto? LeaseExecutionDetails,
    DateOnly? ScheduledClosingDate,
    DateOnly? ActualClosingDate,
    string? ClosingNotes,
    string? FailureReason,
    string? WithdrawalReason,
    string? StalledReason,
    DateOnly? StalledDate,
    DateOnly? ResolutionTargetDate);

/// <summary>The income limit a purchase's home selection judged by.</summary>
/// <param name="PercentOfAmi">The home's limit as a percent of AMI.</param>
/// <param name="EffectiveFrom">The effective date of the schedule the amounts came from.</param>
/// <param name="Limits">The schedule's limits at that percent for 1 to 8 people, in that order.</param>
public sealed record PurchaseIncomeLimitDto(int PercentOfAmi, DateOnly EffectiveFrom, IReadOnlyList<decimal> Limits);

/// <summary>Eligibility verification details.</summary>
/// <param name="VerifiedDate">Date eligibility was verified.</param>
/// <param name="Notes">Optional notes.</param>
/// <param name="VerifiedAnnualIncome">The household's annual income as the documents showed it.</param>
/// <param name="VerifiedHouseholdSize">The number of people in the household as the documents showed it.</param>
/// <param name="IncomeLimit">The limit for that size the income was checked against, or null when the purchase has no limit.</param>
public sealed record EligibilityDetailsDto(
    DateOnly VerifiedDate,
    string? Notes,
    decimal VerifiedAnnualIncome,
    int VerifiedHouseholdSize,
    decimal? IncomeLimit);

/// <summary>Appraisal details.</summary>
/// <param name="AppraisedValue">The home's market value without the lease's restrictions.</param>
/// <param name="AppraisalDate">Date of the appraisal.</param>
/// <param name="AppraiserName">Name of the appraiser.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record AppraisalDetailsDto(decimal AppraisedValue, DateOnly AppraisalDate, string AppraiserName, string? Notes);

/// <summary>Financing commitment details.</summary>
/// <param name="LenderName">Name of the lender.</param>
/// <param name="CommittedAmount">Amount committed.</param>
/// <param name="PreapprovalLetterDate">Date of pre-approval letter.</param>
public sealed record FinancingDetailsDto(string LenderName, decimal CommittedAmount, DateOnly PreapprovalLetterDate);

/// <summary>Inspection result details.</summary>
/// <param name="InspectionDate">Date of inspection.</param>
/// <param name="InspectorName">Name of inspector.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record InspectionResultDto(DateOnly InspectionDate, string InspectorName, string? Notes);

/// <summary>Title clearance details.</summary>
/// <param name="ClearedDate">Date title was cleared.</param>
/// <param name="TitleCompanyName">Optional title company name.</param>
public sealed record TitleDetailsDto(DateOnly ClearedDate, string? TitleCompanyName);

/// <summary>Lease execution details.</summary>
/// <param name="SignedDate">Date lease was signed.</param>
public sealed record LeaseExecutionDetailsDto(DateOnly SignedDate);
