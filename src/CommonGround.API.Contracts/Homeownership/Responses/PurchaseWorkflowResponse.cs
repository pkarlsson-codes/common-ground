namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>
/// Detail response for a purchase workflow. <c>DeclaredAnnualIncome</c> and <c>DeclaredHouseholdSize</c> are what the
/// household declared when selected, and <c>IncomeLimit</c> the limit the home's selection judged by; all null when the
/// purchase didn't start from a selection, and <c>IncomeLimit</c> null when the home had no limit.
/// <c>IncomeLimitSchedule</c> is the schedule in effect when the selection started, every level of it, which the
/// income band is worked out from; null when none was in effect or the purchase didn't start from a selection.
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
    PurchaseIncomeLimitScheduleDto? IncomeLimitSchedule,
    EligibilityDetailsDto? EligibilityDetails,
    AppraisalDetailsDto? AppraisalDetails,
    FinancingDetailsDto? FinancingDetails,
    InspectionResultDto? InspectionResult,
    TitleDetailsDto? TitleDetails,
    LeaseExecutionDetailsDto? LeaseExecutionDetails,
    DisclosureDetailsDto? DisclosureDetails,
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

/// <summary>The income limit schedule a purchase's home selection started with.</summary>
/// <param name="EffectiveFrom">The date the schedule applies from.</param>
/// <param name="Levels">Its levels, lowest percent first.</param>
public sealed record PurchaseIncomeLimitScheduleDto(DateOnly EffectiveFrom, IReadOnlyList<PurchaseIncomeLimitLevelDto> Levels);

/// <summary>One level of a purchase's income limit schedule.</summary>
/// <param name="PercentOfAmi">The percent of area median income.</param>
/// <param name="Limits">The limits for 1 to 8 people, in that order.</param>
public sealed record PurchaseIncomeLimitLevelDto(int PercentOfAmi, IReadOnlyList<decimal> Limits);

/// <summary>Eligibility verification details.</summary>
/// <param name="VerifiedDate">Date eligibility was verified.</param>
/// <param name="Notes">Optional notes.</param>
/// <param name="VerifiedAnnualIncome">The household's annual income as the documents showed it.</param>
/// <param name="VerifiedHouseholdSize">The number of people in the household as the documents showed it.</param>
/// <param name="IncomeLimit">The limit for that size the income was checked against, or null when the purchase has no limit.</param>
/// <param name="IncomeBandOverPercentOfAmi">The level the income is over, or null for the lowest band or when no band was recorded.</param>
/// <param name="IncomeBandUpToPercentOfAmi">The level the income is at or under, or null above the highest level or when no band was recorded.</param>
/// <param name="IncomeDocumentIds">The Documents the income was verified from.</param>
public sealed record EligibilityDetailsDto(
    DateOnly VerifiedDate,
    string? Notes,
    decimal VerifiedAnnualIncome,
    int VerifiedHouseholdSize,
    decimal? IncomeLimit,
    int? IncomeBandOverPercentOfAmi,
    int? IncomeBandUpToPercentOfAmi,
    IReadOnlyList<string> IncomeDocumentIds);

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

/// <summary>The buyer's signed disclosure acknowledgment.</summary>
/// <param name="AcknowledgedDate">The date the buyer signed.</param>
/// <param name="Notes">Optional notes.</param>
/// <param name="DocumentId">The signed acknowledgment in Documents.</param>
public sealed record DisclosureDetailsDto(DateOnly AcknowledgedDate, string? Notes, string DocumentId);
