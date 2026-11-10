using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.Resales;
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

            public static BusinessRuleViolationException ReceivedDateBeforeLeaseStart(DateOnly receivedDate, DateOnly leaseStartDate)
                => new(
                    "homeownership.ground_lease.record_lease_fee_payment.received_date_before_lease_start",
                    new Dictionary<string, object?>
                    {
                        ["receivedDate"] = receivedDate,
                        ["leaseStartDate"] = leaseStartDate,
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
