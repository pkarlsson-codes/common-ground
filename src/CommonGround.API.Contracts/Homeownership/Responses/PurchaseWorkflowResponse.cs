namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Detail response for a purchase workflow.</summary>
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
    EligibilityDetailsDto? EligibilityDetails,
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

/// <summary>Eligibility verification details.</summary>
/// <param name="VerifiedDate">Date eligibility was verified.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record EligibilityDetailsDto(DateOnly VerifiedDate, string? Notes);

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
