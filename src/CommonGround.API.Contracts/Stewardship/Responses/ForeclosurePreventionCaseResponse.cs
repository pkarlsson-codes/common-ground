namespace CommonGround.API.Contracts.Stewardship.Responses;

/// <summary>Detail response for a foreclosure prevention case.</summary>
public sealed record ForeclosurePreventionCaseResponse(
    string Id,
    string GroundLeaseId,
    string Trigger,
    string Status,
    string? Outcome,
    DateOnly OpenedDate,
    DateOnly? ResolvedDate,
    IReadOnlyList<ServicerDelinquencyResponse> ServicerDelinquencies,
    IReadOnlyList<CounselingReferralResponse> CounselingReferrals,
    IReadOnlyList<ForbearanceRequestResponse> ForbearanceRequests,
    IReadOnlyList<CurePeriodActivationResponse> CurePeriodActivations);

/// <summary>Servicer delinquency entry within a foreclosure prevention case.</summary>
public sealed record ServicerDelinquencyResponse(
    string ServicerName,
    int DelinquentMonths,
    DateOnly RecordedDate);

/// <summary>Counseling referral entry within a foreclosure prevention case.</summary>
public sealed record CounselingReferralResponse(
    string Id,
    string AgencyName,
    DateOnly ReferralDate,
    string? Outcome,
    DateOnly? OutcomeDate);

/// <summary>Forbearance request entry within a foreclosure prevention case.</summary>
public sealed record ForbearanceRequestResponse(
    string Id,
    string LenderName,
    DateOnly RequestedDate,
    string? Outcome,
    DateOnly? OutcomeDate,
    string? Notes);

/// <summary>Cure period activation entry within a foreclosure prevention case.</summary>
public sealed record CurePeriodActivationResponse(
    string Id,
    string LraId,
    DateOnly ActivationDate,
    int CurePeriodDays,
    DateOnly CureDeadline);
