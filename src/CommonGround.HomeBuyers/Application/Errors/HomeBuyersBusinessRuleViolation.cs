using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.IncomeLimitSchedules;
using CommonGround.SharedKernel.Domain;
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

            public static BusinessRuleViolationException QualificationToConfirm(DateOnly since)
                => new(
                    "home_buyers.homeowner_application.select_applicant.qualification_to_confirm",
                    new Dictionary<string, object?>
                    {
                        ["since"] = since,
                    });

            public static BusinessRuleViolationException PurchasePriceNonPositive(decimal purchasePrice)
                => new(
                    "home_buyers.homeowner_application.select_applicant.purchase_price_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["purchasePrice"] = purchasePrice,
                    });
        }

        public static class RemoveFromWaitingList
        {
            private const string Prefix = "home_buyers.homeowner_application.remove_from_waiting_list.";

            public static BusinessRuleViolationException PhaseNotApproved(ApplicationPhase phase)
                => new(
                    Prefix + "phase_not_approved",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Approved.DisplayName,
                    });

            public static BusinessRuleViolationException NoteRequired()
                => new(Prefix + "note_required", new Dictionary<string, object?>());

            public static BusinessRuleViolationException RemovedDateInFuture(DateOnly removedDate, DateOnly today)
                => new(
                    Prefix + "removed_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["removedDate"] = removedDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException RemovedDateBeforeApproval(DateOnly removedDate, DateOnly approvedDate)
                => new(
                    Prefix + "removed_date_before_approval",
                    new Dictionary<string, object?>
                    {
                        ["removedDate"] = removedDate,
                        ["approvedDate"] = approvedDate,
                    });
        }

        public static class ConfirmQualification
        {
            private const string Prefix = "home_buyers.homeowner_application.confirm_qualification.";

            public static BusinessRuleViolationException PhaseNotApproved(ApplicationPhase phase)
                => new(
                    Prefix + "phase_not_approved",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Approved.DisplayName,
                    });

            public static BusinessRuleViolationException NothingToConfirm()
                => new(Prefix + "nothing_to_confirm", new Dictionary<string, object?>());

            public static BusinessRuleViolationException ConfirmedDateInFuture(DateOnly confirmedDate, DateOnly today)
                => new(
                    Prefix + "confirmed_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["confirmedDate"] = confirmedDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException ConfirmedDateBeforeChange(DateOnly confirmedDate, DateOnly since)
                => new(
                    Prefix + "confirmed_date_before_change",
                    new Dictionary<string, object?>
                    {
                        ["confirmedDate"] = confirmedDate,
                        ["since"] = since,
                    });
        }

        public static class ReturnToWaitingList
        {
            public static BusinessRuleViolationException PhaseNotSelected(ApplicationPhase phase)
                => new(
                    "home_buyers.homeowner_application.return_to_waiting_list.phase_not_selected",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Selected.DisplayName,
                    });
        }

        public static class RecordPurchaseClosed
        {
            public static BusinessRuleViolationException PhaseNotSelected(ApplicationPhase phase)
                => new(
                    "home_buyers.homeowner_application.record_purchase_closed.phase_not_selected",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = ApplicationPhase.Selected.DisplayName,
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

            public static BusinessRuleViolationException SelectedForPurchase()
                => new("home_buyers.homeowner_application.modify.selected_for_purchase", new Dictionary<string, object?>());
        }
    }

    public static class HomeSelection
    {
        public static class Choose
        {
            public static BusinessRuleViolationException HomeNotAvailable(string homeId, string saleStage)
                => new(
                    "home_buyers.home_selection.choose.home_not_available",
                    new Dictionary<string, object?>
                    {
                        ["homeId"] = homeId,
                        ["saleStage"] = saleStage,
                    });

            public static BusinessRuleViolationException SelectedDateInFuture(DateOnly selectedDate, DateOnly today)
                => new(
                    "home_buyers.home_selection.choose.selected_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["selectedDate"] = selectedDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException SelectedDateBeforeAvailable(DateOnly selectedDate, DateOnly availableSince)
                => new(
                    "home_buyers.home_selection.choose.selected_date_before_available",
                    new Dictionary<string, object?>
                    {
                        ["selectedDate"] = selectedDate,
                        ["availableSince"] = availableSince,
                    });

            public static BusinessRuleViolationException WaitingListEmpty(string homeId)
                => new(
                    "home_buyers.home_selection.choose.waiting_list_empty",
                    new Dictionary<string, object?>
                    {
                        ["homeId"] = homeId,
                    });

            public static BusinessRuleViolationException NoIncomeLimitsInEffect(string homeId, int percentOfAmi, DateOnly selectedDate)
                => new(
                    "home_buyers.home_selection.choose.no_income_limits_in_effect",
                    new Dictionary<string, object?>
                    {
                        ["homeId"] = homeId,
                        ["percentOfAmi"] = percentOfAmi,
                        ["selectedDate"] = selectedDate,
                    });

            public static BusinessRuleViolationException NotOnWaitingList(HomeownerApplicationId applicationId)
                => new(
                    "home_buyers.home_selection.choose.not_on_waiting_list",
                    new Dictionary<string, object?>
                    {
                        ["applicationId"] = applicationId.Value,
                    });

            public static BusinessRuleViolationException CannotBeChosen(
                HomeownerApplicationId applicationId,
                IEnumerable<CannotBeChosenReason> reasons)
                => new(
                    "home_buyers.home_selection.choose.cannot_be_chosen",
                    new Dictionary<string, object?>
                    {
                        ["applicationId"] = applicationId.Value,
                        ["reasons"] = string.Join(", ", reasons?.Select(r => r.DisplayName) ?? []),
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

            public static BusinessRuleViolationException NoEligibleCandidates(int poolSize)
                => new(
                    "home_buyers.home_selection.draw_lottery.no_eligible_candidates",
                    new Dictionary<string, object?>
                    {
                        ["poolSize"] = poolSize,
                    });
        }
    }

    public static class IncomeLimitSchedule
    {
        public static class Levels
        {
            public static BusinessRuleViolationException Required()
                => new(
                    "home_buyers.income_limit_schedule.levels.required");

            public static BusinessRuleViolationException DuplicatePercent(int percentOfAmi)
                => new(
                    "home_buyers.income_limit_schedule.levels.duplicate_percent",
                    new Dictionary<string, object?>
                    {
                        ["percentOfAmi"] = percentOfAmi,
                    });
        }

        public static class EffectiveFrom
        {
            public static BusinessRuleViolationException Taken(DateOnly effectiveFrom, IncomeLimitScheduleId scheduleId)
                => new(
                    "home_buyers.income_limit_schedule.effective_from.taken",
                    new Dictionary<string, object?>
                    {
                        ["effectiveFrom"] = effectiveFrom,
                        ["scheduleId"] = scheduleId.Value,
                    });
        }
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
            public static BusinessRuleViolationException AnnualIncomeNegative(Money annualIncome)
                => new(
                    "home_buyers.household.circumstances.annual_income_negative",
                    new Dictionary<string, object?>
                    {
                        ["annualIncome"] = annualIncome.Amount,
                    });
        }

        public static class CltOrientation
        {
            public static BusinessRuleViolationException CompletedDateInFuture(DateOnly completedDate, DateOnly today)
                => new(
                    "home_buyers.household.clt_orientation.completed_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["completedDate"] = completedDate,
                        ["today"] = today,
                    });
        }

        public static class HousingCounseling
        {
            public static BusinessRuleViolationException CompletedDateInFuture(DateOnly completedDate, DateOnly today)
                => new(
                    "home_buyers.household.housing_counseling.completed_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["completedDate"] = completedDate,
                        ["today"] = today,
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
