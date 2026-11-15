using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.HeirTransfers;
using CommonGround.Homeownership.Domain.ImprovementRequests;
using CommonGround.Homeownership.Domain.MortgageRequests;
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

        public static class RecordDisclosureAcknowledged
        {
            public static BusinessRuleViolationException DocumentRequired()
                => new("homeownership.purchase_workflow.record_disclosure_acknowledged.document_required");

            public static BusinessRuleViolationException AcknowledgedDateInFuture(DateOnly acknowledgedDate, DateOnly today)
                => new(
                    "homeownership.purchase_workflow.record_disclosure_acknowledged.acknowledged_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["acknowledgedDate"] = acknowledgedDate,
                        ["today"] = today,
                    });
        }

        public static class RecordEligibilityVerified
        {
            public static BusinessRuleViolationException IncomeDocumentsRequired()
                => new("homeownership.purchase_workflow.record_eligibility_verified.income_documents_required");

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

        public static class RecordResalePriceStatement
        {
            private const string Prefix = "homeownership.ground_lease.record_resale_price_statement.";

            public static BusinessRuleViolationException StatusNotActive(GroundLeaseStatus status)
                => new(
                    Prefix + "status_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = GroundLeaseStatus.Active.DisplayName,
                    });

            public static BusinessRuleViolationException ResaleInProgress(string resaleId)
                => new(
                    Prefix + "resale_in_progress",
                    new Dictionary<string, object?>
                    {
                        ["resaleId"] = resaleId,
                    });

            public static BusinessRuleViolationException StatementDateInFuture(DateOnly statementDate, DateOnly today)
                => new(
                    Prefix + "statement_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["statementDate"] = statementDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException StatementDateBeforeLeaseStart(DateOnly statementDate, DateOnly leaseStartDate)
                => new(
                    Prefix + "statement_date_before_lease_start",
                    new Dictionary<string, object?>
                    {
                        ["statementDate"] = statementDate,
                        ["leaseStartDate"] = leaseStartDate,
                    });

            public static BusinessRuleViolationException OriginalAppraisedValueMissing()
                => new(Prefix + "original_appraised_value_missing");

            public static BusinessRuleViolationException MarketValueRequired()
                => new(Prefix + "market_value_required");

            public static BusinessRuleViolationException MarketValueNonPositive(decimal marketValue)
                => new(
                    Prefix + "market_value_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["marketValue"] = marketValue,
                    });

            public static BusinessRuleViolationException MarketValueSourceRequired()
                => new(Prefix + "market_value_source_required");

            public static BusinessRuleViolationException IndexValueRequired()
                => new(Prefix + "index_value_required");

            public static BusinessRuleViolationException IndexValueNonPositive(decimal indexValue)
                => new(
                    Prefix + "index_value_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["indexValue"] = indexValue,
                    });

            public static BusinessRuleViolationException ImprovementsCreditNegative(decimal improvementsCredit)
                => new(
                    Prefix + "improvements_credit_negative",
                    new Dictionary<string, object?>
                    {
                        ["improvementsCredit"] = improvementsCredit,
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

        public static class SetInsuranceRequirements
        {
            public static BusinessRuleViolationException StatusNotActive(GroundLeaseStatus status)
                => new(
                    "homeownership.ground_lease.set_insurance_requirements.status_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = GroundLeaseStatus.Active.DisplayName,
                    });

            public static BusinessRuleViolationException EffectiveFromBeforeLeaseStart(DateOnly effectiveFrom, DateOnly leaseStartDate)
                => new(
                    "homeownership.ground_lease.set_insurance_requirements.effective_from_before_lease_start",
                    new Dictionary<string, object?>
                    {
                        ["effectiveFrom"] = effectiveFrom,
                        ["leaseStartDate"] = leaseStartDate,
                    });

            public static BusinessRuleViolationException EffectiveFromBeforeLatestChange(DateOnly effectiveFrom, DateOnly latestEffectiveFrom)
                => new(
                    "homeownership.ground_lease.set_insurance_requirements.effective_from_before_latest_change",
                    new Dictionary<string, object?>
                    {
                        ["effectiveFrom"] = effectiveFrom,
                        ["latestEffectiveFrom"] = latestEffectiveFrom,
                    });
        }

        public static class RecordInsuranceCertificate
        {
            public static BusinessRuleViolationException StatusNotActive(GroundLeaseStatus status)
                => new(
                    "homeownership.ground_lease.record_insurance_certificate.status_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                        ["requiredStatus"] = GroundLeaseStatus.Active.DisplayName,
                    });

            public static BusinessRuleViolationException InsurerRequired()
                => new("homeownership.ground_lease.record_insurance_certificate.insurer_required");

            public static BusinessRuleViolationException PolicyNumberRequired()
                => new("homeownership.ground_lease.record_insurance_certificate.policy_number_required");

            public static BusinessRuleViolationException PolicyEndNotAfterStart(DateOnly policyStart, DateOnly policyEnd)
                => new(
                    "homeownership.ground_lease.record_insurance_certificate.policy_end_not_after_start",
                    new Dictionary<string, object?>
                    {
                        ["policyStart"] = policyStart,
                        ["policyEnd"] = policyEnd,
                    });

            public static BusinessRuleViolationException DwellingCoverageNegative(decimal dwellingCoverage)
                => new(
                    "homeownership.ground_lease.record_insurance_certificate.dwelling_coverage_negative",
                    new Dictionary<string, object?>
                    {
                        ["dwellingCoverage"] = dwellingCoverage,
                    });

            public static BusinessRuleViolationException LiabilityPerOccurrenceNegative(decimal liabilityPerOccurrence)
                => new(
                    "homeownership.ground_lease.record_insurance_certificate.liability_per_occurrence_negative",
                    new Dictionary<string, object?>
                    {
                        ["liabilityPerOccurrence"] = liabilityPerOccurrence,
                    });

            public static BusinessRuleViolationException ReceivedOnInFuture(DateOnly receivedOn, DateOnly today)
                => new(
                    "homeownership.ground_lease.record_insurance_certificate.received_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["receivedOn"] = receivedOn,
                        ["today"] = today,
                    });
        }

        public static class RemoveInsuranceCertificate
        {
            public static BusinessRuleViolationException ReasonRequired()
                => new("homeownership.ground_lease.remove_insurance_certificate.reason_required");

            public static BusinessRuleViolationException CertificateNotFound(string certificateId)
                => new(
                    "homeownership.ground_lease.remove_insurance_certificate.certificate_not_found",
                    new Dictionary<string, object?>
                    {
                        ["certificateId"] = certificateId,
                    });

            public static BusinessRuleViolationException AlreadyRemoved(string certificateId)
                => new(
                    "homeownership.ground_lease.remove_insurance_certificate.already_removed",
                    new Dictionary<string, object?>
                    {
                        ["certificateId"] = certificateId,
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

    public static class InsuranceRequirements
    {
        public static class Create
        {
            public static BusinessRuleViolationException ReplacementValueNegative(decimal replacementValue)
                => new(
                    "homeownership.insurance_requirements.create.replacement_value_negative",
                    new Dictionary<string, object?>
                    {
                        ["replacementValue"] = replacementValue,
                    });

            public static BusinessRuleViolationException LiabilityPerOccurrenceNegative(decimal liabilityPerOccurrence)
                => new(
                    "homeownership.insurance_requirements.create.liability_per_occurrence_negative",
                    new Dictionary<string, object?>
                    {
                        ["liabilityPerOccurrence"] = liabilityPerOccurrence,
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

    public static class MortgageLoanTerms
    {
        public static class Create
        {
            public static BusinessRuleViolationException LenderRequired()
                => new(
                    "homeownership.mortgage_loan_terms.create.lender_required");

            public static BusinessRuleViolationException PrincipalNonPositive(decimal principal)
                => new(
                    "homeownership.mortgage_loan_terms.create.principal_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["principal"] = principal,
                    });

            public static BusinessRuleViolationException TotalDebtBelowPrincipal(decimal totalDebtAfterClosing, decimal principal)
                => new(
                    "homeownership.mortgage_loan_terms.create.total_debt_below_principal",
                    new Dictionary<string, object?>
                    {
                        ["totalDebtAfterClosing"] = totalDebtAfterClosing,
                        ["principal"] = principal,
                    });

            public static BusinessRuleViolationException InterestRateOutOfRange(decimal interestRatePercent)
                => new(
                    "homeownership.mortgage_loan_terms.create.interest_rate_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["interestRatePercent"] = interestRatePercent,
                    });

            public static BusinessRuleViolationException TermYearsOutOfRange(int termYears)
                => new(
                    "homeownership.mortgage_loan_terms.create.term_years_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["termYears"] = termYears,
                    });
        }
    }

    public static class LoanAppraisal
    {
        public static class Create
        {
            public static BusinessRuleViolationException AppraisedValueNonPositive(decimal appraisedValue)
                => new(
                    "homeownership.loan_appraisal.create.appraised_value_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["appraisedValue"] = appraisedValue,
                    });

            public static BusinessRuleViolationException CurrentIndexValueNonPositive(decimal currentIndexValue)
                => new(
                    "homeownership.loan_appraisal.create.current_index_value_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["currentIndexValue"] = currentIndexValue,
                    });
        }
    }

    public static class MortgageRequest
    {
        public static BusinessRuleViolationException ClosingCostsNegative(decimal closingCosts)
            => new(
                "homeownership.mortgage_request.closing_costs_negative",
                new Dictionary<string, object?>
                {
                    ["closingCosts"] = closingCosts,
                });

        public static class Receive
        {
            public static BusinessRuleViolationException LeaseNotActive(GroundLeaseStatus status)
                => new(
                    "homeownership.mortgage_request.receive.lease_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException RequestPending(string pendingRequestId)
                => new(
                    "homeownership.mortgage_request.receive.request_pending",
                    new Dictionary<string, object?>
                    {
                        ["pendingRequestId"] = pendingRequestId,
                    });

            public static BusinessRuleViolationException ReceivedOnInFuture(DateOnly receivedOn, DateOnly today)
                => new(
                    "homeownership.mortgage_request.receive.received_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["receivedOn"] = receivedOn,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException ReceivedOnBeforeLeaseStart(DateOnly receivedOn, DateOnly leaseStartDate)
                => new(
                    "homeownership.mortgage_request.receive.received_on_before_lease_start",
                    new Dictionary<string, object?>
                    {
                        ["receivedOn"] = receivedOn,
                        ["leaseStartDate"] = leaseStartDate,
                    });

            public static BusinessRuleViolationException ExpectedClosingBeforeReceived(DateOnly expectedClosingOn, DateOnly receivedOn)
                => new(
                    "homeownership.mortgage_request.receive.expected_closing_before_received",
                    new Dictionary<string, object?>
                    {
                        ["expectedClosingOn"] = expectedClosingOn,
                        ["receivedOn"] = receivedOn,
                    });
        }

        public static class RecordAppraisal
        {
            public static BusinessRuleViolationException NotPending(MortgageRequestStatus status)
                => new(
                    "homeownership.mortgage_request.record_appraisal.not_pending",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException AppraisalDateInFuture(DateOnly appraisalDate, DateOnly today)
                => new(
                    "homeownership.mortgage_request.record_appraisal.appraisal_date_in_future",
                    new Dictionary<string, object?>
                    {
                        ["appraisalDate"] = appraisalDate,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException LeaseFiguresMissing()
                => new(
                    "homeownership.mortgage_request.record_appraisal.lease_figures_missing");

            public static BusinessRuleViolationException PurchaseOptionPriceNonPositive(decimal purchaseOptionPrice)
                => new(
                    "homeownership.mortgage_request.record_appraisal.purchase_option_price_non_positive",
                    new Dictionary<string, object?>
                    {
                        ["purchaseOptionPrice"] = purchaseOptionPrice,
                    });
        }

        public static class Approve
        {
            public static BusinessRuleViolationException NotPending(MortgageRequestStatus status)
                => new(
                    "homeownership.mortgage_request.approve.not_pending",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException AppraisalRequired()
                => new(
                    "homeownership.mortgage_request.approve.appraisal_required");

            public static BusinessRuleViolationException ApprovedOnBeforeReceived(DateOnly approvedOn, DateOnly receivedOn)
                => new(
                    "homeownership.mortgage_request.approve.approved_on_before_received",
                    new Dictionary<string, object?>
                    {
                        ["approvedOn"] = approvedOn,
                        ["receivedOn"] = receivedOn,
                    });

            public static BusinessRuleViolationException ApprovedOnInFuture(DateOnly approvedOn, DateOnly today)
                => new(
                    "homeownership.mortgage_request.approve.approved_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["approvedOn"] = approvedOn,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException OverCapReasonRequired(decimal debtPercent, int capPercent)
                => new(
                    "homeownership.mortgage_request.approve.over_cap_reason_required",
                    new Dictionary<string, object?>
                    {
                        ["debtPercent"] = debtPercent,
                        ["capPercent"] = capPercent,
                    });
        }

        public static class Deny
        {
            public static BusinessRuleViolationException NotPending(MortgageRequestStatus status)
                => new(
                    "homeownership.mortgage_request.deny.not_pending",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException ReasonRequired()
                => new(
                    "homeownership.mortgage_request.deny.reason_required");

            public static BusinessRuleViolationException DeniedOnBeforeReceived(DateOnly deniedOn, DateOnly receivedOn)
                => new(
                    "homeownership.mortgage_request.deny.denied_on_before_received",
                    new Dictionary<string, object?>
                    {
                        ["deniedOn"] = deniedOn,
                        ["receivedOn"] = receivedOn,
                    });

            public static BusinessRuleViolationException DeniedOnInFuture(DateOnly deniedOn, DateOnly today)
                => new(
                    "homeownership.mortgage_request.deny.denied_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["deniedOn"] = deniedOn,
                        ["today"] = today,
                    });
        }

        public static class RecordLoanClosed
        {
            public static BusinessRuleViolationException NotApproved(MortgageRequestStatus status)
                => new(
                    "homeownership.mortgage_request.record_loan_closed.not_approved",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException ClosedOnBeforeApproved(DateOnly closedOn, DateOnly approvedOn)
                => new(
                    "homeownership.mortgage_request.record_loan_closed.closed_on_before_approved",
                    new Dictionary<string, object?>
                    {
                        ["closedOn"] = closedOn,
                        ["approvedOn"] = approvedOn,
                    });

            public static BusinessRuleViolationException ClosedOnInFuture(DateOnly closedOn, DateOnly today)
                => new(
                    "homeownership.mortgage_request.record_loan_closed.closed_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["closedOn"] = closedOn,
                        ["today"] = today,
                    });
        }

        public static class Withdraw
        {
            public static BusinessRuleViolationException NotOpen(MortgageRequestStatus status)
                => new(
                    "homeownership.mortgage_request.withdraw.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException ReasonRequired()
                => new(
                    "homeownership.mortgage_request.withdraw.reason_required");

            public static BusinessRuleViolationException WithdrawnOnBeforeReceived(DateOnly withdrawnOn, DateOnly receivedOn)
                => new(
                    "homeownership.mortgage_request.withdraw.withdrawn_on_before_received",
                    new Dictionary<string, object?>
                    {
                        ["withdrawnOn"] = withdrawnOn,
                        ["receivedOn"] = receivedOn,
                    });

            public static BusinessRuleViolationException WithdrawnOnInFuture(DateOnly withdrawnOn, DateOnly today)
                => new(
                    "homeownership.mortgage_request.withdraw.withdrawn_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["withdrawnOn"] = withdrawnOn,
                        ["today"] = today,
                    });
        }
    }

    public static class ImprovementRequest
    {
        public static class Receive
        {
            public static BusinessRuleViolationException LeaseNotActive(GroundLeaseStatus status)
                => new(
                    "homeownership.improvement_request.receive.lease_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException DescriptionRequired()
                => new("homeownership.improvement_request.receive.description_required");

            public static BusinessRuleViolationException EstimatedCostNotPositive(decimal estimatedCost)
                => new(
                    "homeownership.improvement_request.receive.estimated_cost_not_positive",
                    new Dictionary<string, object?>
                    {
                        ["estimatedCost"] = estimatedCost,
                    });

            public static BusinessRuleViolationException ReceivedOnInFuture(DateOnly receivedOn, DateOnly today)
                => new(
                    "homeownership.improvement_request.receive.received_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["receivedOn"] = receivedOn,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException ReceivedOnBeforeLeaseStart(DateOnly receivedOn, DateOnly leaseStartDate)
                => new(
                    "homeownership.improvement_request.receive.received_on_before_lease_start",
                    new Dictionary<string, object?>
                    {
                        ["receivedOn"] = receivedOn,
                        ["leaseStartDate"] = leaseStartDate,
                    });
        }

        public static class Approve
        {
            public static BusinessRuleViolationException NotWaiting(ImprovementRequestStatus status)
                => new(
                    "homeownership.improvement_request.approve.not_waiting",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException CreditOutOfRange(decimal preApprovedCredit, decimal estimatedCost)
                => new(
                    "homeownership.improvement_request.approve.credit_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["preApprovedCredit"] = preApprovedCredit,
                        ["estimatedCost"] = estimatedCost,
                    });

            public static BusinessRuleViolationException ApprovedOnBeforeReceived(DateOnly approvedOn, DateOnly receivedOn)
                => new(
                    "homeownership.improvement_request.approve.approved_on_before_received",
                    new Dictionary<string, object?>
                    {
                        ["approvedOn"] = approvedOn,
                        ["receivedOn"] = receivedOn,
                    });

            public static BusinessRuleViolationException ApprovedOnInFuture(DateOnly approvedOn, DateOnly today)
                => new(
                    "homeownership.improvement_request.approve.approved_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["approvedOn"] = approvedOn,
                        ["today"] = today,
                    });
        }

        public static class Deny
        {
            public static BusinessRuleViolationException NotWaiting(ImprovementRequestStatus status)
                => new(
                    "homeownership.improvement_request.deny.not_waiting",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException ReasonRequired()
                => new("homeownership.improvement_request.deny.reason_required");

            public static BusinessRuleViolationException DeniedOnBeforeReceived(DateOnly deniedOn, DateOnly receivedOn)
                => new(
                    "homeownership.improvement_request.deny.denied_on_before_received",
                    new Dictionary<string, object?>
                    {
                        ["deniedOn"] = deniedOn,
                        ["receivedOn"] = receivedOn,
                    });

            public static BusinessRuleViolationException DeniedOnInFuture(DateOnly deniedOn, DateOnly today)
                => new(
                    "homeownership.improvement_request.deny.denied_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["deniedOn"] = deniedOn,
                        ["today"] = today,
                    });
        }

        public static class RecordCompleted
        {
            public static BusinessRuleViolationException NotApproved(ImprovementRequestStatus status)
                => new(
                    "homeownership.improvement_request.record_completed.not_approved",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException ReceiptsRequired()
                => new("homeownership.improvement_request.record_completed.receipts_required");

            public static BusinessRuleViolationException ActualCostNotPositive(decimal actualCost)
                => new(
                    "homeownership.improvement_request.record_completed.actual_cost_not_positive",
                    new Dictionary<string, object?>
                    {
                        ["actualCost"] = actualCost,
                    });

            public static BusinessRuleViolationException CreditOutOfRange(decimal confirmedCredit, decimal preApprovedCredit)
                => new(
                    "homeownership.improvement_request.record_completed.credit_out_of_range",
                    new Dictionary<string, object?>
                    {
                        ["confirmedCredit"] = confirmedCredit,
                        ["preApprovedCredit"] = preApprovedCredit,
                    });

            public static BusinessRuleViolationException CompletedOnBeforeApproved(DateOnly completedOn, DateOnly approvedOn)
                => new(
                    "homeownership.improvement_request.record_completed.completed_on_before_approved",
                    new Dictionary<string, object?>
                    {
                        ["completedOn"] = completedOn,
                        ["approvedOn"] = approvedOn,
                    });

            public static BusinessRuleViolationException CompletedOnInFuture(DateOnly completedOn, DateOnly today)
                => new(
                    "homeownership.improvement_request.record_completed.completed_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["completedOn"] = completedOn,
                        ["today"] = today,
                    });
        }

        public static class Withdraw
        {
            public static BusinessRuleViolationException NotOpen(ImprovementRequestStatus status)
                => new(
                    "homeownership.improvement_request.withdraw.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException ReasonRequired()
                => new("homeownership.improvement_request.withdraw.reason_required");

            public static BusinessRuleViolationException WithdrawnOnBeforeReceived(DateOnly withdrawnOn, DateOnly receivedOn)
                => new(
                    "homeownership.improvement_request.withdraw.withdrawn_on_before_received",
                    new Dictionary<string, object?>
                    {
                        ["withdrawnOn"] = withdrawnOn,
                        ["receivedOn"] = receivedOn,
                    });

            public static BusinessRuleViolationException WithdrawnOnInFuture(DateOnly withdrawnOn, DateOnly today)
                => new(
                    "homeownership.improvement_request.withdraw.withdrawn_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["withdrawnOn"] = withdrawnOn,
                        ["today"] = today,
                    });
        }
    }

    public static class HeirTransfer
    {
        public static class Open
        {
            public static BusinessRuleViolationException LeaseNotActive(GroundLeaseStatus status)
                => new(
                    "homeownership.heir_transfer.open.lease_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException AlreadyOpen(string openTransferId)
                => new(
                    "homeownership.heir_transfer.open.already_open",
                    new Dictionary<string, object?>
                    {
                        ["openTransferId"] = openTransferId,
                    });

            public static BusinessRuleViolationException NotifiedByRequired()
                => new("homeownership.heir_transfer.open.notified_by_required");

            public static BusinessRuleViolationException DiedOnInFuture(DateOnly diedOn, DateOnly today)
                => new(
                    "homeownership.heir_transfer.open.died_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["diedOn"] = diedOn,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException NoticeBeforeDeath(DateOnly noticeReceivedOn, DateOnly diedOn)
                => new(
                    "homeownership.heir_transfer.open.notice_before_death",
                    new Dictionary<string, object?>
                    {
                        ["noticeReceivedOn"] = noticeReceivedOn,
                        ["diedOn"] = diedOn,
                    });

            public static BusinessRuleViolationException NoticeInFuture(DateOnly noticeReceivedOn, DateOnly today)
                => new(
                    "homeownership.heir_transfer.open.notice_in_future",
                    new Dictionary<string, object?>
                    {
                        ["noticeReceivedOn"] = noticeReceivedOn,
                        ["today"] = today,
                    });
        }

        public static class RecordHeir
        {
            public static BusinessRuleViolationException NotOpen(HeirTransferStatus status)
                => new(
                    "homeownership.heir_transfer.record_heir.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException MovedInOnRequired()
                => new("homeownership.heir_transfer.record_heir.moved_in_on_required");

            public static BusinessRuleViolationException HouseholdMemberUnderOneYear(DateOnly movedInOn, DateOnly diedOn)
                => new(
                    "homeownership.heir_transfer.record_heir.household_member_under_one_year",
                    new Dictionary<string, object?>
                    {
                        ["movedInOn"] = movedInOn,
                        ["diedOn"] = diedOn,
                    });

            public static BusinessRuleViolationException IncomeRequired()
                => new("homeownership.heir_transfer.record_heir.income_required");

            public static BusinessRuleViolationException HouseholdSizeRequired()
                => new("homeownership.heir_transfer.record_heir.household_size_required");

            public static BusinessRuleViolationException IncomeDocumentsRequired()
                => new("homeownership.heir_transfer.record_heir.income_documents_required");

            public static BusinessRuleViolationException IncomeNegative(decimal income)
                => new(
                    "homeownership.heir_transfer.record_heir.income_negative",
                    new Dictionary<string, object?>
                    {
                        ["income"] = income,
                    });

            public static BusinessRuleViolationException HouseholdSizeBelowOne(int householdSize)
                => new(
                    "homeownership.heir_transfer.record_heir.household_size_below_one",
                    new Dictionary<string, object?>
                    {
                        ["householdSize"] = householdSize,
                    });

            public static BusinessRuleViolationException IncomeLimitsMissing(int percentOfAmi, DateOnly on)
                => new(
                    "homeownership.heir_transfer.record_heir.income_limits_missing",
                    new Dictionary<string, object?>
                    {
                        ["percentOfAmi"] = percentOfAmi,
                        ["on"] = on,
                    });

            public static BusinessRuleViolationException OverIncomeLimit(
                decimal income, decimal limit, int percentOfAmi, int householdSize, DateOnly effectiveFrom)
                => new(
                    "homeownership.heir_transfer.record_heir.over_income_limit",
                    new Dictionary<string, object?>
                    {
                        ["income"] = income,
                        ["limit"] = limit,
                        ["percentOfAmi"] = percentOfAmi,
                        ["householdSize"] = householdSize,
                        ["effectiveFrom"] = effectiveFrom,
                    });
        }

        public static class Complete
        {
            public static BusinessRuleViolationException NoHeirRecorded(HeirTransferStatus status)
                => new(
                    "homeownership.heir_transfer.complete.no_heir_recorded",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException LettersRequired()
                => new("homeownership.heir_transfer.complete.letters_required");

            public static BusinessRuleViolationException TransferredOnBeforeDeath(DateOnly transferredOn, DateOnly diedOn)
                => new(
                    "homeownership.heir_transfer.complete.transferred_on_before_death",
                    new Dictionary<string, object?>
                    {
                        ["transferredOn"] = transferredOn,
                        ["diedOn"] = diedOn,
                    });

            public static BusinessRuleViolationException TransferredOnInFuture(DateOnly transferredOn, DateOnly today)
                => new(
                    "homeownership.heir_transfer.complete.transferred_on_in_future",
                    new Dictionary<string, object?>
                    {
                        ["transferredOn"] = transferredOn,
                        ["today"] = today,
                    });

            public static BusinessRuleViolationException LeaseNotActive(GroundLeaseStatus status)
                => new(
                    "homeownership.heir_transfer.complete.lease_not_active",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });

            public static BusinessRuleViolationException OpenResale(string resaleId)
                => new(
                    "homeownership.heir_transfer.complete.open_resale",
                    new Dictionary<string, object?>
                    {
                        ["resaleId"] = resaleId,
                    });
        }

        public static class Close
        {
            public static BusinessRuleViolationException NotOpen(HeirTransferStatus status)
                => new(
                    "homeownership.heir_transfer.close.not_open",
                    new Dictionary<string, object?>
                    {
                        ["status"] = status.DisplayName,
                    });
        }
    }
}
