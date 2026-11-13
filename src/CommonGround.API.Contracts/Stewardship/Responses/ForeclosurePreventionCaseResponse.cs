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
    LeaseFeeArrearsResponse? LeaseFeeArrears,
    IReadOnlyList<ServicerDelinquencyResponse> ServicerDelinquencies,
    IReadOnlyList<CounselingReferralResponse> CounselingReferrals,
    IReadOnlyList<ForbearanceRequestResponse> ForbearanceRequests,
    IReadOnlyList<CurePeriodActivationResponse> CurePeriodActivations);

/// <summary>The unpaid notice of default a case opened from a lease default was opened for.</summary>
/// <param name="NoticeSentOn">The day the notice of default was sent.</param>
/// <param name="AmountOwed">What the notice said was owed.</param>
/// <param name="CureBy">The last day to pay the amount owed, after any extension.</param>
/// <param name="StillToPay">What was still to pay of the amount owed when the case opened.</param>
public sealed record LeaseFeeArrearsResponse(
    DateOnly NoticeSentOn,
    decimal AmountOwed,
    DateOnly CureBy,
    decimal StillToPay);

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
