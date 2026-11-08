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
