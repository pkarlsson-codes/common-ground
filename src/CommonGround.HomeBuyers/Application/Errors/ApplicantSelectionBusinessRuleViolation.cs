using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.HomeBuyers.Application.Errors;

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
            public static BusinessRuleViolationException PhaseNotApproved(ApplicationPhase phase)
                => new(
                    "applicant_selection.homeowner_application.select_applicant.phase_not_approved",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Approved.DisplayName,
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

    public static class HomeAvailabilityClaim
    {
        public static BusinessRuleViolationException AlreadyClaimed(string homeId)
            => new(
                "applicant_selection.home_availability_claim.already_claimed",
                new Dictionary<string, object?>
                {
                    ["homeId"] = homeId,
                });
    }

    public static class Household
    {
        public static class Register
        {
            public static BusinessRuleViolationException MustHaveExactlyOnePrimaryMember()
                => new(
                    "applicant_selection.household.register.must_have_exactly_one_primary_member");
        }

        public static class SelectApplicant
        {
            public static BusinessRuleViolationException NoPrimaryMember(HouseholdId householdId)
                => new(
                    "applicant_selection.household.select_applicant.no_primary_member",
                    new Dictionary<string, object?>
                    {
                        ["householdId"] = householdId.Value,
                    });
        }
    }
}
