using CommonGround.API.Contracts.Stewardship.Requests;
using CommonGround.API.Contracts.Stewardship.Responses;

namespace CommonGround.Web.Gateway.Clients;

/// <summary>Typed HTTP client for the Stewardship API endpoints.</summary>
public interface IStewardshipApiClient
{
    // Stewardship Cases

    /// <summary>Gets a stewardship case by its ID.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stewardship case details.</returns>
    Task<StewardshipCaseResponse> GetStewardshipCaseAsync(string caseId, CancellationToken cancellationToken);

    /// <summary>Gets all stewardship cases for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The list of stewardship cases.</returns>
    Task<IReadOnlyList<StewardshipCaseResponse>> GetStewardshipCasesByGroundLeaseAsync(string groundLeaseId, CancellationToken cancellationToken);

    /// <summary>Initiates an annual occupancy verification.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="request">The verification initiation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InitiateOccupancyVerificationAsync(string caseId, InitiateOccupancyVerificationRequest request, CancellationToken cancellationToken);

    /// <summary>Submits homeowner responses to an occupancy verification.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="verificationId">The ID of the verification.</param>
    /// <param name="request">The submission details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SubmitOccupancyVerificationAsync(string caseId, Guid verificationId, SubmitOccupancyVerificationRequest request, CancellationToken cancellationToken);

    /// <summary>Records staff approval of an occupancy verification.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="verificationId">The ID of the verification.</param>
    /// <param name="request">The verification approval details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task VerifyOccupancyAsync(string caseId, Guid verificationId, VerifyOccupancyRequest request, CancellationToken cancellationToken);

    /// <summary>Records a failed occupancy verification.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="verificationId">The ID of the verification.</param>
    /// <param name="request">The failure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task FailOccupancyAsync(string caseId, Guid verificationId, FailOccupancyRequest request, CancellationToken cancellationToken);

    /// <summary>Marks an occupancy verification as overdue.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="verificationId">The ID of the verification.</param>
    /// <param name="request">The overdue details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkOccupancyVerificationOverdueAsync(string caseId, Guid verificationId, MarkOccupancyVerificationOverdueRequest request, CancellationToken cancellationToken);

    /// <summary>Schedules a property inspection.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="request">The inspection scheduling details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SchedulePropertyInspectionAsync(string caseId, SchedulePropertyInspectionRequest request, CancellationToken cancellationToken);

    /// <summary>Records the completion of a property inspection.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="inspectionId">The ID of the inspection.</param>
    /// <param name="request">The inspection completion details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CompletePropertyInspectionAsync(string caseId, Guid inspectionId, CompletePropertyInspectionRequest request, CancellationToken cancellationToken);

    /// <summary>Cancels a scheduled property inspection.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="inspectionId">The ID of the inspection.</param>
    /// <param name="request">The cancellation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CancelPropertyInspectionAsync(string caseId, Guid inspectionId, CancelPropertyInspectionRequest request, CancellationToken cancellationToken);

    /// <summary>Identifies a new lease violation.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="request">The violation identification details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task IdentifyViolationAsync(string caseId, IdentifyViolationRequest request, CancellationToken cancellationToken);

    /// <summary>Issues a formal notice for a lease violation.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="violationId">The ID of the violation.</param>
    /// <param name="request">The notice details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task IssueViolationNoticeAsync(string caseId, Guid violationId, IssueViolationNoticeRequest request, CancellationToken cancellationToken);

    /// <summary>Records that a lease violation has been cured.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="violationId">The ID of the violation.</param>
    /// <param name="request">The cure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CureViolationAsync(string caseId, Guid violationId, CureViolationRequest request, CancellationToken cancellationToken);

    /// <summary>Escalates an unresolved lease violation.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="violationId">The ID of the violation.</param>
    /// <param name="request">The escalation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EscalateViolationAsync(string caseId, Guid violationId, EscalateViolationRequest request, CancellationToken cancellationToken);

    /// <summary>Closes a stewardship case.</summary>
    /// <param name="caseId">The ID of the stewardship case.</param>
    /// <param name="request">The closure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CloseStewardshipCaseAsync(string caseId, CloseStewardshipCaseRequest request, CancellationToken cancellationToken);

    // Foreclosure Prevention Cases

    /// <summary>Opens a new foreclosure prevention case for servicer delinquency.</summary>
    /// <param name="request">The case opening details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response containing the new case ID.</returns>
    Task<OpenForeclosurePreventionCaseResponse> OpenForeclosurePreventionCaseAsync(OpenForeclosurePreventionCaseRequest request, CancellationToken cancellationToken);

    /// <summary>Gets a foreclosure prevention case by its ID.</summary>
    /// <param name="caseId">The ID of the foreclosure prevention case.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The foreclosure prevention case details.</returns>
    Task<ForeclosurePreventionCaseResponse> GetForeclosurePreventionCaseAsync(string caseId, CancellationToken cancellationToken);

    /// <summary>Gets all foreclosure prevention cases for a ground lease.</summary>
    /// <param name="groundLeaseId">The ID of the ground lease.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The list of foreclosure prevention cases.</returns>
    Task<IReadOnlyList<ForeclosurePreventionCaseResponse>> GetForeclosurePreventionCasesByGroundLeaseAsync(string groundLeaseId, CancellationToken cancellationToken);

    /// <summary>Records a servicer delinquency on a foreclosure prevention case.</summary>
    /// <param name="caseId">The ID of the foreclosure prevention case.</param>
    /// <param name="request">The delinquency details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordServicerDelinquencyAsync(string caseId, RecordServicerDelinquencyRequest request, CancellationToken cancellationToken);

    /// <summary>Makes a housing counseling referral on a foreclosure prevention case.</summary>
    /// <param name="caseId">The ID of the foreclosure prevention case.</param>
    /// <param name="request">The referral details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MakeCounselingReferralAsync(string caseId, MakeCounselingReferralRequest request, CancellationToken cancellationToken);

    /// <summary>Records the outcome of a counseling referral.</summary>
    /// <param name="caseId">The ID of the foreclosure prevention case.</param>
    /// <param name="referralId">The ID of the referral.</param>
    /// <param name="request">The outcome details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordCounselingOutcomeAsync(string caseId, Guid referralId, RecordCounselingOutcomeRequest request, CancellationToken cancellationToken);

    /// <summary>Submits a mortgage forbearance request to a lender.</summary>
    /// <param name="caseId">The ID of the foreclosure prevention case.</param>
    /// <param name="request">The forbearance request details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RequestForbearanceAsync(string caseId, RequestForbearanceRequest request, CancellationToken cancellationToken);

    /// <summary>Records the outcome of a forbearance request.</summary>
    /// <param name="caseId">The ID of the foreclosure prevention case.</param>
    /// <param name="requestId">The ID of the forbearance request.</param>
    /// <param name="request">The outcome details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RecordForbearanceOutcomeAsync(string caseId, Guid requestId, RecordForbearanceOutcomeRequest request, CancellationToken cancellationToken);

    /// <summary>Activates an LRA cure period on a foreclosure prevention case.</summary>
    /// <param name="caseId">The ID of the foreclosure prevention case.</param>
    /// <param name="request">The cure period activation details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ActivateCurePeriodAsync(string caseId, ActivateCurePeriodRequest request, CancellationToken cancellationToken);

    /// <summary>Resolves a foreclosure prevention case with a final outcome.</summary>
    /// <param name="caseId">The ID of the foreclosure prevention case.</param>
    /// <param name="request">The resolution details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResolveForeclosurePreventionCaseAsync(string caseId, ResolveForeclosurePreventionCaseRequest request, CancellationToken cancellationToken);
}
