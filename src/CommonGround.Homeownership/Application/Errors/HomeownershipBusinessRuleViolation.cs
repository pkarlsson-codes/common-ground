using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.Resales;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Homeownership.Application.Errors;

public static class BusinessRuleViolation
{
    public static class PurchaseWorkflow
    {
        public static class Initiate
        {
            public static BusinessRuleViolationException PurchasePriceNonPositive(decimal purchasePrice)
                => new(
                    "homeownership.purchase_workflow.initiate.purchase_price_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["purchasePrice"] = purchasePrice,
                    });
        }

        public static class RecordEligibilityVerified
        {
            public static BusinessRuleViolationException IncomeNegative(Money income)
                => new(
                    "homeownership.purchase_workflow.record_eligibility_verified.income_negative",
                    new Dictionary<string, object?>
                    {
                        ["income"] = income,
                    });

            public static BusinessRuleViolationException HouseholdSizeBelowOne(int householdSize)
                => new(
                    "homeownership.purchase_workflow.record_eligibility_verified.household_size_below_one",
                    new Dictionary<string, object?>
                    {
                        ["householdSize"] = householdSize,
                    });

            public static BusinessRuleViolationException OverIncomeLimit(
                Money income, Money limit, int percentOfAmi, int householdSize, DateOnly effectiveFrom)
                => new(
                    "homeownership.purchase_workflow.record_eligibility_verified.over_income_limit",
                    new Dictionary<string, object?>
                    {
                        ["income"] = income,
                        ["limit"] = limit,
                        ["percentOfAmi"] = percentOfAmi,
                        ["householdSize"] = householdSize,
                        ["effectiveFrom"] = effectiveFrom,
                    });
        }

        public static class RecordAppraisalCompleted
        {
            public static BusinessRuleViolationException AppraisedValueNonPositive(decimal appraisedValue)
                => new(
                    "homeownership.purchase_workflow.record_appraisal_completed.appraised_value_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["appraisedValue"] = appraisedValue,
                    });
        }

        public static class ScheduleClosing
        {
            public static BusinessRuleViolationException MandatoryTracksIncomplete()
                => new(
                    "homeownership.purchase_workflow.schedule_closing.mandatory_tracks_incomplete");

            public static BusinessRuleViolationException InvalidPhase(WorkflowPhase phase)
                => new(
                    "homeownership.purchase_workflow.schedule_closing.invalid_phase",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = WorkflowPhase.InProgress.DisplayName,
                    });
        }

        public static class RecordPurchaseClosed
        {
            public static BusinessRuleViolationException PhaseNotClosingScheduled(WorkflowPhase phase)
                => new(
                    "homeownership.purchase_workflow.record_purchase_closed.phase_not_closing_scheduled",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = WorkflowPhase.ClosingScheduled.DisplayName,
                    });
        }

        public static class FailPurchase
        {
            public static BusinessRuleViolationException FailureReasonRequired()
                => new(
                    "homeownership.purchase_workflow.fail_purchase.failure_reason_required");
        }

        public static class StallWorkflow
        {
            public static BusinessRuleViolationException PhaseNotInProgress(WorkflowPhase phase)
                => new(
                    "homeownership.purchase_workflow.stall_workflow.phase_not_in_progress",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                        ["requiredPhase"] = WorkflowPhase.InProgress.DisplayName,
                    });
        }

        public static class Modify
        {
            public static BusinessRuleViolationException TerminalPhase(WorkflowPhase phase)
                => new(
                    "homeownership.purchase_workflow.modify.terminal_phase",
                    new Dictionary<string, object?>
                    {
                        ["phase"] = phase.DisplayName,
                    });
        }
    }

    public static class GroundLease
    {
        public static class Create
        {
            public static BusinessRuleViolationException TermYearsNonPositive(int termYears)
                => new(
                    "homeownership.ground_lease.create.term_years_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["termYears"] = termYears,
                    });

            public static BusinessRuleViolationException OptionWindowDaysNonPositive(int optionWindowDays)
                => new(
                    "homeownership.ground_lease.create.option_window_days_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["optionWindowDays"] = optionWindowDays,
                    });

            public static BusinessRuleViolationException BasePriceNonPositive(decimal basePrice)
                => new(
                    "homeownership.ground_lease.create.base_price_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["basePrice"] = basePrice,
                    });

            public static BusinessRuleViolationException OriginalAppraisedValueNonPositive(decimal originalAppraisedValue)
                => new(
                    "homeownership.ground_lease.create.original_appraised_value_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["originalAppraisedValue"] = originalAppraisedValue,
                    });

            public static BusinessRuleViolationException CovenantTermsRequired()
                => new(
                    "homeownership.ground_lease.create.covenant_terms_required");

            public static BusinessRuleViolationException HomeHasActiveLease(string groundLeaseId)
                => new(
                    "homeownership.ground_lease.create.home_has_active_lease",
                    new Dictionary<string, object?>
                    {
                        ["groundLeaseId"] = groundLeaseId,
                    });

            public static BusinessRuleViolationException HomeHasOpenPurchase(string purchaseWorkflowId)
                => new(
                    "homeownership.ground_lease.create.home_has_open_purchase",
                    new Dictionary<string, object?>
                    {
                        ["purchaseWorkflowId"] = purchaseWorkflowId,
                    });

            public static BusinessRuleViolationException HomeInSelection(string homeSelectionId)
                => new(
                    "homeownership.ground_lease.create.home_in_selection",
                    new Dictionary<string, object?>
                    {
                        ["homeSelectionId"] = homeSelectionId,
                    });
        }

        public static class CreateFromTemplate
        {
            public static BusinessRuleViolationException TemplateNotActive(GroundLeaseTemplateStatus status)
                => new(
                    "homeownership.ground_lease.create_from_template.template_not_active",
                    new Dictionary<string, object?>
                    {
                        ["templateStatus"] = status.DisplayName,
                        ["requiredStatus"] = GroundLeaseTemplateStatus.Active.DisplayName,
                    });
        }

        public static class OnboardHomeowner
        {
            public static BusinessRuleViolationException AlreadyOnboarded()
                => new(
                    "homeownership.ground_lease.onboard_homeowner.already_onboarded");
        }

        public static class ChangeHomeowner
        {
            public static BusinessRuleViolationException CurrentHomeownerMissing()
                => new(
                    "homeownership.ground_lease.change_homeowner.current_homeowner_missing");
        }

        public static class Terminate
        {
            public static BusinessRuleViolationException ReasonRequired()
                => new(
                    "homeownership.ground_lease.terminate.reason_required");

            public static BusinessRuleViolationException StatusNotActive(GroundLeaseStatus status)
                => new(
                    "homeownership.ground_lease.terminate.status_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = GroundLeaseStatus.Active.DisplayName,
                    });
        }

        public static class RecordStewardshipCheckIn
        {
            public static BusinessRuleViolationException NotesRequired()
                => new(
                    "homeownership.ground_lease.record_stewardship_checkin.notes_required");
        }

        public static class StartResale
        {
            public static BusinessRuleViolationException StatusNotActive(GroundLeaseStatus status)
                => new(
                    "homeownership.ground_lease.start_resale.status_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = GroundLeaseStatus.Active.DisplayName,
                    });

            public static BusinessRuleViolationException ResaleAlreadyOpen(string resaleId)
                => new(
                    "homeownership.ground_lease.start_resale.resale_already_open",
                    new Dictionary<string, object?>
                    {
                        ["resaleId"] = resaleId,
                    });
        }

        public static class RecordLeaseFeePayment
        {
            public static BusinessRuleViolationException AmountNonPositive(decimal amount)
                => new(
                    "homeownership.ground_lease.record_lease_fee_payment.amount_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["amount"] = amount,
                    });

            public static BusinessRuleViolationException ReceivedDateInFuture(DateOnly receivedDate, DateOnly today)
                => new(
                    "homeownership.ground_lease.record_lease_fee_payment.received_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["receivedDate"] = receivedDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException ReceivedDateBeforeFeesTracked(DateOnly receivedDate, DateOnly feesTrackedFrom)
                => new(
                    "homeownership.ground_lease.record_lease_fee_payment.received_date_before_fees_tracked",
                    new Dictionary<string, object?>
                    {
                        ["receivedDate"] = receivedDate,
                        ["feesTrackedFrom"] = feesTrackedFrom,
                    });
        }

        public static class RecordLeaseFeeNotice
        {
            public static BusinessRuleViolationException SentDateInFuture(DateOnly sentOn, DateOnly today)
                => new(
                    "homeownership.ground_lease.record_lease_fee_notice.sent_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["sentOn"] = sentOn,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException SentDateBeforeLeaseStart(DateOnly sentOn, DateOnly leaseStartDate)
                => new(
                    "homeownership.ground_lease.record_lease_fee_notice.sent_date_before_lease_start",
                    new Dictionary<string, object?>
                    {
                        ["sentOn"] = sentOn,
                        ["leaseStartDate"] = leaseStartDate,
                    });

            public static BusinessRuleViolationException NothingOwed(DateOnly sentOn)
                => new(
                    "homeownership.ground_lease.record_lease_fee_notice.nothing_owed",
                    new Dictionary<string, object?>
                    {
                        ["sentOn"] = sentOn,
                    });

            public static BusinessRuleViolationException DefaultNoticeDeliveryNotAllowed(NoticeDelivery delivery)
                => new(
                    "homeownership.ground_lease.record_lease_fee_notice.default_notice_delivery_not_allowed",
                    new Dictionary<string, object?>
                    {
                        ["delivery"] = delivery.DisplayName,
                    });

            public static BusinessRuleViolationException DefaultNoticeAlreadyCuring(DateOnly cureBy)
                => new(
                    "homeownership.ground_lease.record_lease_fee_notice.default_notice_already_curing",
                    new Dictionary<string, object?>
                    {
                        ["cureBy"] = cureBy,
                    });
        }

        public static class WaiveLeaseFeeInterest
        {
            public static BusinessRuleViolationException ReasonRequired()
                => new("homeownership.ground_lease.waive_lease_fee_interest.reason_required");

            public static BusinessRuleViolationException AmountNonPositive(decimal amount)
                => new(
                    "homeownership.ground_lease.waive_lease_fee_interest.amount_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["amount"] = amount,
                    });

            public static BusinessRuleViolationException AmountAboveInterestOwed(decimal amount, decimal interestOwed)
                => new(
                    "homeownership.ground_lease.waive_lease_fee_interest.amount_above_interest_owed",
                    new Dictionary<string, object?>
                    {
                        ["amount"] = amount,
                        ["interestOwed"] = interestOwed,
                    });
        }

        public static class RecordPermittedMortgage
        {
            public static BusinessRuleViolationException LenderRequired()
                => new("homeownership.ground_lease.record_permitted_mortgage.lender_required");

            public static BusinessRuleViolationException PrincipalNonPositive(decimal principal)
                => new(
                    "homeownership.ground_lease.record_permitted_mortgage.principal_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["principal"] = principal,
                    });

            public static BusinessRuleViolationException PermittedOnInFuture(DateOnly permittedOn, DateOnly today)
                => new(
                    "homeownership.ground_lease.record_permitted_mortgage.permitted_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["permittedOn"] = permittedOn,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException PermittedOnBeforeLeaseStart(DateOnly permittedOn, DateOnly leaseStartDate)
                => new(
                    "homeownership.ground_lease.record_permitted_mortgage.permitted_on_before_lease_start",
                    new Dictionary<string, object?>
                    {
                        ["permittedOn"] = permittedOn,
                        ["leaseStartDate"] = leaseStartDate,
                    });
        }

        public static class EditPermittedMortgage
        {
            public static BusinessRuleViolationException LenderRequired()
                => new("homeownership.ground_lease.edit_permitted_mortgage.lender_required");

            public static BusinessRuleViolationException MortgageNotFound(string mortgageId)
                => new(
                    "homeownership.ground_lease.edit_permitted_mortgage.mortgage_not_found",
                    new Dictionary<string, object?>
                    {
                        ["mortgageId"] = mortgageId,
                    });

            public static BusinessRuleViolationException MortgagePaidOff(string mortgageId)
                => new(
                    "homeownership.ground_lease.edit_permitted_mortgage.mortgage_paid_off",
                    new Dictionary<string, object?>
                    {
                        ["mortgageId"] = mortgageId,
                    });
        }

        public static class MarkPermittedMortgagePaidOff
        {
            public static BusinessRuleViolationException MortgageNotFound(string mortgageId)
                => new(
                    "homeownership.ground_lease.mark_permitted_mortgage_paid_off.mortgage_not_found",
                    new Dictionary<string, object?>
                    {
                        ["mortgageId"] = mortgageId,
                    });

            public static BusinessRuleViolationException AlreadyPaidOff(string mortgageId)
                => new(
                    "homeownership.ground_lease.mark_permitted_mortgage_paid_off.already_paid_off",
                    new Dictionary<string, object?>
                    {
                        ["mortgageId"] = mortgageId,
                    });

            public static BusinessRuleViolationException PaidOffOnInFuture(DateOnly paidOffOn, DateOnly today)
                => new(
                    "homeownership.ground_lease.mark_permitted_mortgage_paid_off.paid_off_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["paidOffOn"] = paidOffOn,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException PaidOffOnBeforePermitted(DateOnly paidOffOn, DateOnly permittedOn)
                => new(
                    "homeownership.ground_lease.mark_permitted_mortgage_paid_off.paid_off_on_before_permitted",
                    new Dictionary<string, object?>
                    {
                        ["paidOffOn"] = paidOffOn,
                        ["permittedOn"] = permittedOn,
                    });
        }

        public static class CorrectLeaseFeeTracking
        {
            public static BusinessRuleViolationException PaymentBeforeFeesTracked(DateOnly receivedDate, DateOnly feesTrackedFrom)
                => new(
                    "homeownership.ground_lease.correct_lease_fee_tracking.payment_before_fees_tracked",
                    new Dictionary<string, object?>
                    {
                        ["receivedDate"] = receivedDate,
                        ["feesTrackedFrom"] = feesTrackedFrom,
                    });
        }

        public static class VoidLeaseFeePayment
        {
            public static BusinessRuleViolationException ReasonRequired()
                => new(
                    "homeownership.ground_lease.void_lease_fee_payment.reason_required");

            public static BusinessRuleViolationException PaymentNotFound(string paymentId)
                => new(
                    "homeownership.ground_lease.void_lease_fee_payment.payment_not_found",
                    new Dictionary<string, object?>
                    {
                        ["paymentId"] = paymentId,
                    });

            public static BusinessRuleViolationException AlreadyVoided(string paymentId)
                => new(
                    "homeownership.ground_lease.void_lease_fee_payment.already_voided",
                    new Dictionary<string, object?>
                    {
                        ["paymentId"] = paymentId,
                    });
        }
    }

    public static class LeaseFeeNoticeSchedule
    {
        public static class Create
        {
            public static BusinessRuleViolationException FirstNoticeDaysTooFew(int firstNoticeDays)
                => new(
                    "homeownership.lease_fee_notice_schedule.create.first_notice_days_too_few",
                    new Dictionary<string, object?>
                    {
                        ["firstNoticeDays"] = firstNoticeDays,
                    });

            public static BusinessRuleViolationException SecondNoticeNotAfterFirst(int firstNoticeDays, int secondNoticeDays)
                => new(
                    "homeownership.lease_fee_notice_schedule.create.second_notice_not_after_first",
                    new Dictionary<string, object?>
                    {
                        ["firstNoticeDays"] = firstNoticeDays,
                        ["secondNoticeDays"] = secondNoticeDays,
                    });

            public static BusinessRuleViolationException DefaultNoticeNotAfterSecond(int secondNoticeDays, int defaultNoticeDays)
                => new(
                    "homeownership.lease_fee_notice_schedule.create.default_notice_not_after_second",
                    new Dictionary<string, object?>
                    {
                        ["secondNoticeDays"] = secondNoticeDays,
                        ["defaultNoticeDays"] = defaultNoticeDays,
                    });
        }
    }

    public static class DefaultCurePeriod
    {
        public static class Create
        {
            public static BusinessRuleViolationException CureDaysTooFew(int cureDays)
                => new(
                    "homeownership.default_cure_period.create.cure_days_too_few",
                    new Dictionary<string, object?>
                    {
                        ["cureDays"] = cureDays,
                    });

            public static BusinessRuleViolationException ExtensionDaysNegative(int extensionDays)
                => new(
                    "homeownership.default_cure_period.create.extension_days_negative",
                    new Dictionary<string, object?>
                    {
                        ["extensionDays"] = extensionDays,
                    });
        }
    }

    public static class LeaseFeeTerms
    {
        public static class Create
        {
            public static BusinessRuleViolationException LandUseFeeNegative(decimal landUseFee)
                => new(
                    "homeownership.lease_fee_terms.create.land_use_fee_negative",
                    new Dictionary<string, object?>
                    {
                        ["landUseFee"] = landUseFee,
                    });

            public static BusinessRuleViolationException RepairReserveFeeNegative(decimal repairReserveFee)
                => new(
                    "homeownership.lease_fee_terms.create.repair_reserve_fee_negative",
                    new Dictionary<string, object?>
                    {
                        ["repairReserveFee"] = repairReserveFee,
                    });

            public static BusinessRuleViolationException LateInterestRateOutOfRange(decimal lateInterestRate)
                => new(
                    "homeownership.lease_fee_terms.create.late_interest_rate_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["lateInterestRate"] = lateInterestRate,
                    });

            public static BusinessRuleViolationException InterestWaiverDaysNegative(int interestWaiverDays)
                => new(
                    "homeownership.lease_fee_terms.create.interest_waiver_days_negative",
                    new Dictionary<string, object?>
                    {
                        ["interestWaiverDays"] = interestWaiverDays,
                    });
        }
    }

    public static class MortgageDebtCap
    {
        public static class Create
        {
            public static BusinessRuleViolationException PercentOutOfRange(int percent)
                => new(
                    "homeownership.mortgage_debt_cap.create.percent_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["percent"] = percent,
                    });
        }
    }

    public static class LeaseFeeTracking
    {
        public static class Create
        {
            public static BusinessRuleViolationException TrackedFromBeforeLeaseStart(DateOnly trackedFrom, DateOnly leaseStartDate)
                => new(
                    "homeownership.lease_fee_tracking.create.tracked_from_before_lease_start",
                    new Dictionary<string, object?>
                    {
                        ["trackedFrom"] = trackedFrom,
                        ["leaseStartDate"] = leaseStartDate,
                    });

            public static BusinessRuleViolationException OpeningBalanceNegative(decimal openingBalance)
                => new(
                    "homeownership.lease_fee_tracking.create.opening_balance_negative",
                    new Dictionary<string, object?>
                    {
                        ["openingBalance"] = openingBalance,
                    });
        }
    }

    public static class GroundLeaseTemplate
    {
        public static class Create
        {
            public static BusinessRuleViolationException TemplateNameRequired()
                => new(
                    "homeownership.ground_lease_template.create.template_name_required");

            public static BusinessRuleViolationException TermYearsNonPositive(int termYears)
                => new(
                    "homeownership.ground_lease_template.create.term_years_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["termYears"] = termYears,
                    });

            public static BusinessRuleViolationException OptionWindowDaysNonPositive(int optionWindowDays)
                => new(
                    "homeownership.ground_lease_template.create.option_window_days_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["optionWindowDays"] = optionWindowDays,
                    });

            public static BusinessRuleViolationException CovenantTermsRequired()
                => new(
                    "homeownership.ground_lease_template.create.covenant_terms_required");
        }

        public static class Restore
        {
            public static BusinessRuleViolationException AlreadyActive()
                => new(
                    "homeownership.ground_lease_template.restore.already_active");
        }

        public static class Archive
        {
            public static BusinessRuleViolationException AlreadyInactive()
                => new(
                    "homeownership.ground_lease_template.archive.already_inactive");
        }
    }

    public static class Resale
    {
        public static class RecordIntentToSell
        {
            public static BusinessRuleViolationException NoticeDateInFuture(DateOnly noticeDate, DateOnly today)
                => new(
                    "homeownership.resale.record_intent_to_sell.notice_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["noticeDate"] = noticeDate,
                        ["today"] = today,
                    });
        }

        public static class Withdraw
        {
            public static BusinessRuleViolationException NotOpen(ResaleStatus status)
                => new(
                    "homeownership.resale.withdraw.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = ResaleStatus.Open.DisplayName,
                    });

            public static BusinessRuleViolationException ReasonRequired()
                => new(
                    "homeownership.resale.withdraw.reason_required");

            public static BusinessRuleViolationException WithdrawnDateBeforeNotice(DateOnly withdrawnDate, DateOnly noticeDate)
                => new(
                    "homeownership.resale.withdraw.withdrawn_date_before_notice",
                    new Dictionary<string, object?>
                    {
                        ["withdrawnDate"] = withdrawnDate,
                        ["noticeDate"] = noticeDate,
                    });

            public static BusinessRuleViolationException WithdrawnDateInFuture(DateOnly withdrawnDate, DateOnly today)
                => new(
                    "homeownership.resale.withdraw.withdrawn_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["withdrawnDate"] = withdrawnDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException OptionExercised()
                => new(
                    "homeownership.resale.withdraw.option_exercised");
        }

        public static class RecordAppraisal
        {
            public static BusinessRuleViolationException NotOpen(ResaleStatus status)
                => new(
                    "homeownership.resale.record_appraisal.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = ResaleStatus.Open.DisplayName,
                    });

            public static BusinessRuleViolationException OptionExercised()
                => new(
                    "homeownership.resale.record_appraisal.option_exercised");

            public static BusinessRuleViolationException LeaseFiguresMissing()
                => new(
                    "homeownership.resale.record_appraisal.lease_figures_missing");

            public static BusinessRuleViolationException AppraisedValueNonPositive(decimal appraisedValue)
                => new(
                    "homeownership.resale.record_appraisal.appraised_value_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["appraisedValue"] = appraisedValue,
                    });

            public static BusinessRuleViolationException AppraiserRequired()
                => new(
                    "homeownership.resale.record_appraisal.appraiser_required");

            public static BusinessRuleViolationException AppraisalDateBeforeNotice(DateOnly appraisalDate, DateOnly noticeDate)
                => new(
                    "homeownership.resale.record_appraisal.appraisal_date_before_notice",
                    new Dictionary<string, object?>
                    {
                        ["appraisalDate"] = appraisalDate,
                        ["noticeDate"] = noticeDate,
                    });

            public static BusinessRuleViolationException AppraisalDateInFuture(DateOnly appraisalDate, DateOnly today)
                => new(
                    "homeownership.resale.record_appraisal.appraisal_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["appraisalDate"] = appraisalDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException ApprovedImprovementsNegative(decimal approvedImprovements)
                => new(
                    "homeownership.resale.record_appraisal.approved_improvements_negative",
                    new Dictionary<string, object?>
                    {
                        ["approvedImprovements"] = approvedImprovements,
                    });

            public static BusinessRuleViolationException CurrentIndexValueNonPositive(decimal currentIndexValue)
                => new(
                    "homeownership.resale.record_appraisal.current_index_value_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["currentIndexValue"] = currentIndexValue,
                    });
        }

        public static class ExerciseOption
        {
            public static BusinessRuleViolationException NotOpen(ResaleStatus status)
                => new(
                    "homeownership.resale.exercise_option.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = ResaleStatus.Open.DisplayName,
                    });

            public static BusinessRuleViolationException AppraisalRequired()
                => new(
                    "homeownership.resale.exercise_option.appraisal_required");

            public static BusinessRuleViolationException AlreadyExercised()
                => new(
                    "homeownership.resale.exercise_option.already_exercised");

            public static BusinessRuleViolationException ExercisedDateBeforeAppraisal(DateOnly exercisedDate, DateOnly appraisalDate)
                => new(
                    "homeownership.resale.exercise_option.exercised_date_before_appraisal",
                    new Dictionary<string, object?>
                    {
                        ["exercisedDate"] = exercisedDate,
                        ["appraisalDate"] = appraisalDate,
                    });

            public static BusinessRuleViolationException ExercisedDateInFuture(DateOnly exercisedDate, DateOnly today)
                => new(
                    "homeownership.resale.exercise_option.exercised_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["exercisedDate"] = exercisedDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException OptionWindowEnded(DateOnly optionWindowEndsOn)
                => new(
                    "homeownership.resale.exercise_option.option_window_ended",
                    new Dictionary<string, object?>
                    {
                        ["optionWindowEndsOn"] = optionWindowEndsOn,
                    });
        }

        public static class CloseBuyback
        {
            public static BusinessRuleViolationException NotOpen(ResaleStatus status)
                => new(
                    "homeownership.resale.close_buyback.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = ResaleStatus.Open.DisplayName,
                    });

            public static BusinessRuleViolationException OptionNotExercised()
                => new(
                    "homeownership.resale.close_buyback.option_not_exercised");

            public static BusinessRuleViolationException ClosingDateBeforeExercised(DateOnly closingDate, DateOnly exercisedDate)
                => new(
                    "homeownership.resale.close_buyback.closing_date_before_exercised",
                    new Dictionary<string, object?>
                    {
                        ["closingDate"] = closingDate,
                        ["exercisedDate"] = exercisedDate,
                    });

            public static BusinessRuleViolationException ClosingDateInFuture(DateOnly closingDate, DateOnly today)
                => new(
                    "homeownership.resale.close_buyback.closing_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["closingDate"] = closingDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException PricePaidNonPositive(decimal pricePaid)
                => new(
                    "homeownership.resale.close_buyback.price_paid_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["pricePaid"] = pricePaid,
                    });

            public static BusinessRuleViolationException PricePaidAboveOptionPrice(decimal pricePaid, decimal purchaseOptionPrice)
                => new(
                    "homeownership.resale.close_buyback.price_paid_above_option_price",
                    new Dictionary<string, object?>
                    {
                        ["pricePaid"] = pricePaid,
                        ["purchaseOptionPrice"] = purchaseOptionPrice,
                    });

            public static BusinessRuleViolationException HoldReasonRequired()
                => new(
                    "homeownership.resale.close_buyback.hold_reason_required");
        }
    }

    public static class ResalePolicyTerms
    {
        public static class AppraisalBased
        {
            public static BusinessRuleViolationException AppreciationShareOutOfRange(decimal appreciationShare)
                => new(
                    "homeownership.resale_policy_terms.appraisal_based.appreciation_share_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["appreciationShare"] = appreciationShare,
                        ["min"] = 0.0m,
                        ["max"] = 1.0m,
                    });
        }

        public static class IndexBased
        {
            public static BusinessRuleViolationException IndexTypeRequired()
                => new(
                    "homeownership.resale_policy_terms.index_based.index_type_required");

            public static BusinessRuleViolationException BaselineIndexNonPositive(decimal baselineIndexValue)
                => new(
                    "homeownership.resale_policy_terms.index_based.baseline_index_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["baselineIndexValue"] = baselineIndexValue,
                    });

            public static BusinessRuleViolationException CurrentIndexValueRequired()
                => new(
                    "homeownership.resale_policy_terms.index_based.current_index_value_required");
        }

        public static class FixedPercentage
        {
            public static BusinessRuleViolationException AnnualPercentageOutOfRange(decimal annualPercentage)
                => new(
                    "homeownership.resale_policy_terms.fixed_percentage.annual_percentage_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["annualPercentage"] = annualPercentage,
                        ["min"] = 0.0m,
                        ["max"] = 1.0m,
                    });
        }

        public static class Create
        {
            public static BusinessRuleViolationException ImprovementsCapNonPositive(decimal? improvementsCap)
                => new(
                    "homeownership.resale_policy_terms.create.improvements_cap_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["improvementsCap"] = improvementsCap,
                    });
        }
    }
}
