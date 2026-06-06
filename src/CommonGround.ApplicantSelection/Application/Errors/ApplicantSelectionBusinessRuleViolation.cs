using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.ApplicantSelection.Application.Errors;

public static class BusinessRuleViolation
{
    public static class HomeownerApplication
    {
        public static class ApproveEligibility
        {
            public static BusinessRuleViolationException PhaseNotSubmitted(ApplicationPhase phase)
                => new(
                    "applicant_selection.homeowner_application.approve_eligibility.phase_not_submitted",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Submitted.DisplayName,
                    });
        }

        public static class DenyEligibility
        {
            public static BusinessRuleViolationException PhaseNotSubmitted(ApplicationPhase phase)
                => new(
                    "applicant_selection.homeowner_application.deny_eligibility.phase_not_submitted",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Submitted.DisplayName,
                    });
        }

        public static class SelectApplicant
        {
            public static BusinessRuleViolationException PhaseNotWaitlisted(ApplicationPhase phase)
                => new(
                    "applicant_selection.homeowner_application.select_applicant.phase_not_waitlisted",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Waitlisted.DisplayName,
                    });

            public static BusinessRuleViolationException PrequalificationExpired()
                => new(
                    "applicant_selection.homeowner_application.select_applicant.prequalification_expired");

            public static BusinessRuleViolationException PurchasePriceNonPositive(decimal purchasePrice)
                => new(
                    "applicant_selection.homeowner_application.select_applicant.purchase_price_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["purchasePrice"] = purchasePrice,
                    });
        }

        public static class Modify
        {
            public static BusinessRuleViolationException TerminalPhase(ApplicationPhase phase)
                => new(
                    "applicant_selection.homeowner_application.modify.terminal_phase",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                    });
        }
    }
}
