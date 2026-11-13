using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Stewardship.Domain.ForeclosurePreventionCases;
using CommonGround.Stewardship.Domain.StewardshipCases;

namespace CommonGround.Stewardship.Application.Errors;

public static class BusinessRuleViolation
{
    public static class StewardshipCase
    {
        public static class Open
        {
            public static BusinessRuleViolationException GroundLeaseIdRequired()
                => new("stewardship.stewardship_case.open.ground_lease_id_required");
        }

        public static class SubmitOccupancyVerification
        {
            public static BusinessRuleViolationException InvalidStatus(OccupancyVerificationStatus status)
                => new(
                    "stewardship.stewardship_case.submit_occupancy_verification.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class VerifyOccupancy
        {
            public static BusinessRuleViolationException InvalidStatus(OccupancyVerificationStatus status)
                => new(
                    "stewardship.stewardship_case.verify_occupancy.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class FailOccupancy
        {
            public static BusinessRuleViolationException FailureReasonRequired()
                => new("stewardship.stewardship_case.fail_occupancy.failure_reason_required");

            public static BusinessRuleViolationException InvalidStatus(OccupancyVerificationStatus status)
                => new(
                    "stewardship.stewardship_case.fail_occupancy.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class MarkOccupancyVerificationOverdue
        {
            public static BusinessRuleViolationException InvalidStatus(OccupancyVerificationStatus status)
                => new(
                    "stewardship.stewardship_case.mark_occupancy_verification_overdue.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class SchedulePropertyInspection
        {
            public static BusinessRuleViolationException InspectorNameRequired()
                => new("stewardship.stewardship_case.schedule_property_inspection.inspector_name_required");
        }

        public static class CompletePropertyInspection
        {
            public static BusinessRuleViolationException InvalidStatus(PropertyInspectionStatus status)
                => new(
                    "stewardship.stewardship_case.complete_property_inspection.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class CancelPropertyInspection
        {
            public static BusinessRuleViolationException CancellationReasonRequired()
                => new("stewardship.stewardship_case.cancel_property_inspection.cancellation_reason_required");

            public static BusinessRuleViolationException InvalidStatus(PropertyInspectionStatus status)
                => new(
                    "stewardship.stewardship_case.cancel_property_inspection.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class IdentifyViolation
        {
            public static BusinessRuleViolationException DescriptionRequired()
                => new("stewardship.stewardship_case.identify_violation.description_required");
        }

        public static class IssueViolationNotice
        {
            public static BusinessRuleViolationException InvalidStatus(LeaseViolationStatus status)
                => new(
                    "stewardship.stewardship_case.issue_violation_notice.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class CureViolation
        {
            public static BusinessRuleViolationException InvalidStatus(LeaseViolationStatus status)
                => new(
                    "stewardship.stewardship_case.cure_violation.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class EscalateViolation
        {
            public static BusinessRuleViolationException EscalationReasonRequired()
                => new("stewardship.stewardship_case.escalate_violation.escalation_reason_required");

            public static BusinessRuleViolationException InvalidStatus(LeaseViolationStatus status)
                => new(
                    "stewardship.stewardship_case.escalate_violation.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class Close
        {
            public static BusinessRuleViolationException CloseReasonRequired()
                => new("stewardship.stewardship_case.close.close_reason_required");
        }

        public static class GuardActive
        {
            public static BusinessRuleViolationException InvalidStatus(StewardshipCaseStatus status)
                => new(
                    "stewardship.stewardship_case.guard_active.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class FindVerification
        {
            public static BusinessRuleViolationException NotFound(Guid id)
                => new(
                    "stewardship.stewardship_case.find_verification.not_found",
                    new Dictionary<string, object?>
                    {
                        ["id"] = id,
                    });
        }

        public static class FindInspection
        {
            public static BusinessRuleViolationException NotFound(Guid id)
                => new(
                    "stewardship.stewardship_case.find_inspection.not_found",
                    new Dictionary<string, object?>
                    {
                        ["id"] = id,
                    });
        }

        public static class FindViolation
        {
            public static BusinessRuleViolationException NotFound(Guid id)
                => new(
                    "stewardship.stewardship_case.find_violation.not_found",
                    new Dictionary<string, object?>
                    {
                        ["id"] = id,
                    });
        }
    }

    public static class ForeclosurePreventionCase
    {
        public static class Open
        {
            public static BusinessRuleViolationException GroundLeaseIdRequired()
                => new("stewardship.foreclosure_prevention_case.open.ground_lease_id_required");

            public static BusinessRuleViolationException CaseAlreadyOpen(string groundLeaseId, ForeclosurePreventionCaseId unresolvedCaseId)
                => new(
                    "stewardship.foreclosure_prevention_case.open.case_already_open",
                    new Dictionary<string, object?>
                    {
                        ["groundLeaseId"] = groundLeaseId,
                        ["unresolvedCaseId"] = unresolvedCaseId.Value,
                    });
        }

        public static class OpenForLeaseDefault
        {
            public static BusinessRuleViolationException NoUnpaidDefault(string groundLeaseId)
                => new(
                    "stewardship.foreclosure_prevention_case.open_for_lease_default.no_unpaid_default",
                    new Dictionary<string, object?>
                    {
                        ["groundLeaseId"] = groundLeaseId,
                    });
        }

        public static class RecordServicerDelinquency
        {
            public static BusinessRuleViolationException ServicerNameRequired()
                => new("stewardship.foreclosure_prevention_case.record_servicer_delinquency.servicer_name_required");

            public static BusinessRuleViolationException InvalidDelinquentMonths(int delinquentMonths)
                => new(
                    "stewardship.foreclosure_prevention_case.record_servicer_delinquency.invalid_delinquent_months",
                    new Dictionary<string, object?>
                    {
                        ["delinquentMonths"] = delinquentMonths,
                    });
        }

        public static class MakeCounselingReferral
        {
            public static BusinessRuleViolationException AgencyNameRequired()
                => new("stewardship.foreclosure_prevention_case.make_counseling_referral.agency_name_required");
        }

        public static class RecordCounselingOutcome
        {
            public static BusinessRuleViolationException OutcomeRequired()
                => new("stewardship.foreclosure_prevention_case.record_counseling_outcome.outcome_required");
        }

        public static class RequestForbearance
        {
            public static BusinessRuleViolationException LenderNameRequired()
                => new("stewardship.foreclosure_prevention_case.request_forbearance.lender_name_required");
        }

        public static class ActivateCurePeriod
        {
            public static BusinessRuleViolationException LraIdRequired()
                => new("stewardship.foreclosure_prevention_case.activate_cure_period.lra_id_required");

            public static BusinessRuleViolationException InvalidCurePeriodDays(int curePeriodDays)
                => new(
                    "stewardship.foreclosure_prevention_case.activate_cure_period.invalid_cure_period_days",
                    new Dictionary<string, object?>
                    {
                        ["curePeriodDays"] = curePeriodDays,
                    });
        }

        public static class Resolve
        {
            public static BusinessRuleViolationException InvalidStatus(ForeclosurePreventionCaseStatus status)
                => new(
                    "stewardship.foreclosure_prevention_case.resolve.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class GuardNotResolved
        {
            public static BusinessRuleViolationException InvalidStatus(ForeclosurePreventionCaseStatus status)
                => new(
                    "stewardship.foreclosure_prevention_case.guard_not_resolved.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class GuardActive
        {
            public static BusinessRuleViolationException InvalidStatus(ForeclosurePreventionCaseStatus status)
                => new(
                    "stewardship.foreclosure_prevention_case.guard_active.invalid_status",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.ToString(),
                    });
        }

        public static class FindReferral
        {
            public static BusinessRuleViolationException NotFound(Guid id)
                => new(
                    "stewardship.foreclosure_prevention_case.find_referral.not_found",
                    new Dictionary<string, object?>
                    {
                        ["id"] = id,
                    });
        }

        public static class FindForbearanceRequest
        {
            public static BusinessRuleViolationException NotFound(Guid id)
                => new(
                    "stewardship.foreclosure_prevention_case.find_forbearance_request.not_found",
                    new Dictionary<string, object?>
                    {
                        ["id"] = id,
                    });
        }
    }
}
