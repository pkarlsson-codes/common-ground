namespace CommonGround.API.Contracts.Stewardship.Responses;

/// <summary>Detail response for a stewardship case.</summary>
public sealed record StewardshipCaseResponse(
    string Id,
    string GroundLeaseId,
    string Status,
    DateOnly OpenedDate,
    DateOnly? ClosedDate,
    IReadOnlyList<OccupancyVerificationResponse> OccupancyVerifications,
    IReadOnlyList<PropertyInspectionResponse> PropertyInspections,
    IReadOnlyList<LeaseViolationResponse> LeaseViolations);

/// <summary>Occupancy verification entry within a stewardship case.</summary>
public sealed record OccupancyVerificationResponse(
    string Id,
    int PeriodYear,
    DateOnly? SurveyDeadline,
    string Status,
    bool? IsPrimaryResident,
    bool? SubletDisclosed,
    string? MajorChanges,
    DateOnly? SubmittedDate,
    DateOnly? DecisionDate,
    string? Reason);

/// <summary>Property inspection entry within a stewardship case.</summary>
public sealed record PropertyInspectionResponse(
    string Id,
    DateOnly ScheduledDate,
    string InspectorName,
    string Status,
    bool? OccupancyConfirmed,
    string? ConditionNotes,
    DateOnly? CompletedDate,
    string? CancellationReason);

/// <summary>Lease violation entry within a stewardship case.</summary>
public sealed record LeaseViolationResponse(
    string Id,
    string ViolationType,
    string Description,
    DateOnly IdentifiedDate,
    string Status,
    DateOnly? NoticeIssuedDate,
    DateOnly? CureDeadline,
    DateOnly? ResolutionDate,
    string? ResolutionNotes);
