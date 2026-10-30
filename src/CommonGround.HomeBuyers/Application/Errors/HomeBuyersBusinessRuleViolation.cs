using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.HomeBuyers.Application.Errors;

public static class BusinessRuleViolation
{
    public static class HomeownerApplication
    {
        public static class Approve
        {
            public static BusinessRuleViolationException PhaseNotSubmitted(ApplicationPhase phase)
                => new(
                    "home_buyers.homeowner_application.approve.phase_not_submitted",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Submitted.DisplayName,
                    });
        }

        public static class Deny
        {
            public static BusinessRuleViolationException PhaseNotSubmitted(ApplicationPhase phase)
                => new(
                    "home_buyers.homeowner_application.deny.phase_not_submitted",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Submitted.DisplayName,
                    });

            public static BusinessRuleViolationException ReasonRequired()
                => new(
                    "home_buyers.homeowner_application.deny.reason_required");
        }

        public static class SelectApplicant
        {
            public static BusinessRuleViolationException PhaseNotApproved(ApplicationPhase phase)
                => new(
                    "home_buyers.homeowner_application.select_applicant.phase_not_approved",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Approved.DisplayName,
                    });

            public static BusinessRuleViolationException HouseholdReadinessIncomplete(IReadOnlyList<string> outstandingItems)
                => new(
                    "home_buyers.homeowner_application.select_applicant.household_readiness_incomplete",
                    new Dictionary<string, object?>
                    {
                        ["outstandingItems"] = outstandingItems,
                    });

            public static BusinessRuleViolationException PrequalificationExpired(DateOnly expiryDate, DateOnly selectedDate)
                => new(
                    "home_buyers.homeowner_application.select_applicant.prequalification_expired",
                    new Dictionary<string, object?>
                    {
                        ["expiryDate"] = expiryDate,
                        ["selectedDate"] = selectedDate,
                    });

            public static BusinessRuleViolationException PurchasePriceNonPositive(decimal purchasePrice)
                => new(
                    "home_buyers.homeowner_application.select_applicant.purchase_price_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["purchasePrice"] = purchasePrice,
                    });
        }

        public static class Submit
        {
            public static BusinessRuleViolationException HouseholdHasOpenApplication(HomeownerApplicationId openApplicationId)
                => new(
                    "home_buyers.homeowner_application.submit.household_has_open_application",
                    new Dictionary<string, object?>
                    {
                        ["openApplicationId"] = openApplicationId.Value,
                    });
        }

        public static class Modify
        {
            public static BusinessRuleViolationException TerminalPhase(ApplicationPhase phase)
                => new(
                    "home_buyers.homeowner_application.modify.terminal_phase",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                    });
        }
    }

    public static class HomeSelection
    {
        public static class Start
        {
            public static BusinessRuleViolationException HomeHasActiveSelection(string homeId, HomeSelectionId activeSelectionId)
                => new(
                    "home_buyers.home_selection.start.home_has_active_selection",
                    new Dictionary<string, object?>
                    {
                        ["homeId"] = homeId,
                        ["activeSelectionId"] = activeSelectionId.Value,
                    });

            public static BusinessRuleViolationException WaitingListEmpty(string homeId)
                => new(
                    "home_buyers.home_selection.start.waiting_list_empty",
                    new Dictionary<string, object?>
                    {
                        ["homeId"] = homeId,
                    });
        }

        public static class SelectByPriority
        {
            public static BusinessRuleViolationException PrioritiesRequired()
                => new(
                    "home_buyers.home_selection.select_by_priority.priorities_required");

            public static BusinessRuleViolationException ReasonRequired()
                => new(
                    "home_buyers.home_selection.select_by_priority.reason_required");
        }

        public static class DrawLottery
        {
            public static BusinessRuleViolationException TiedPoolTooSmall(int poolSize)
                => new(
                    "home_buyers.home_selection.draw_lottery.tied_pool_too_small",
                    new Dictionary<string, object?>
                    {
                        ["poolSize"] = poolSize,
                        ["minimumPoolSize"] = 2,
                    });
        }

        public static class Cancel
        {
            public static BusinessRuleViolationException ReasonRequired()
                => new(
                    "home_buyers.home_selection.cancel.reason_required");
        }

        public static class Modify
        {
            public static BusinessRuleViolationException NotOpen(HomeSelectionStatus status)
                => new(
                    "home_buyers.home_selection.modify.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = HomeSelectionStatus.Open.DisplayName,
                    });

            public static BusinessRuleViolationException NotACandidate(HomeownerApplicationId applicationId)
                => new(
                    "home_buyers.home_selection.modify.not_a_candidate",
                    new Dictionary<string, object?>
                    {
                        ["applicationId"] = applicationId.Value,
                    });
        }
    }

    public static class HomeAvailabilityClaim
    {
        public static BusinessRuleViolationException AlreadyClaimed(string homeId)
            => new(
                "home_buyers.home_availability_claim.already_claimed",
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
                    "home_buyers.household.register.must_have_exactly_one_primary_member");
        }

        public static class Members
        {
            public static BusinessRuleViolationException MemberNotFound(HouseholdMemberId memberId)
                => new(
                    "home_buyers.household.members.member_not_found",
                    new Dictionary<string, object?>
                    {
                        ["memberId"] = memberId.Value,
                    });

            public static BusinessRuleViolationException CannotRemovePrimaryMember(HouseholdMemberId memberId)
                => new(
                    "home_buyers.household.members.cannot_remove_primary_member",
                    new Dictionary<string, object?>
                    {
                        ["memberId"] = memberId.Value,
                    });
        }

        public static class Circumstances
        {
            public static BusinessRuleViolationException AnnualIncomeNegative(decimal annualIncome)
                => new(
                    "home_buyers.household.circumstances.annual_income_negative",
                    new Dictionary<string, object?>
                    {
                        ["annualIncome"] = annualIncome,
                    });
        }

        public static class SelectApplicant
        {
            public static BusinessRuleViolationException NoPrimaryMember(HouseholdId householdId)
                => new(
                    "home_buyers.household.select_applicant.no_primary_member",
                    new Dictionary<string, object?>
                    {
                        ["householdId"] = householdId.Value,
                    });
        }
    }
}
