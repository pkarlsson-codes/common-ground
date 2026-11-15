using CommonGround.Homeownership.Domain.GroundLeases;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using CommonGround.Homeownership.Domain.PurchaseWorkflows.TrackDetails;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.PurchaseWorkflows;

public class PurchaseWorkflowCorrectionTests
{
    public class TheCorrectEligibilityMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenARecordedEligibility_WhenCorrected_ThenTheDetailsHoldTheNewValuesAndTheEventHoldsTheReason()
        {
            var sut = _testBed.WithEligibility();

            var corrected = sut.CorrectEligibility(
                _testBed.Date, Money.Create(50_000m), 3, ["new-doc"], "Rechecked", "Pay stub misread", _testBed.StaffUser);
            sut.Apply(corrected);

            corrected.Reason.Should().Be("Pay stub misread");
            corrected.CorrectedBy.Should().Be(_testBed.StaffUser.Value);
            sut.EligibilityDetails.Should().BeEquivalentTo(new
            {
                VerifiedAnnualIncome = Money.Create(50_000m),
                VerifiedHouseholdSize = 3,
                Notes = "Rechecked",
                IncomeDocumentIds = new[] { "new-doc" },
                IncomeLimit = Money.Create(64_300m),
            });
        }

        [Fact]
        public void GivenNoNewDocuments_WhenCorrected_ThenTheRecordedDocumentsStay()
        {
            var sut = _testBed.WithEligibility();

            var corrected = sut.CorrectEligibility(
                _testBed.Date, Money.Create(50_000m), 2, null, null, "Typo", _testBed.StaffUser);

            corrected.IncomeDocumentIds.Should().Equal("income-doc");
        }

        [Fact]
        public void GivenNewDocuments_WhenCorrected_ThenTheyReplaceTheRecordedOnes()
        {
            var sut = _testBed.WithEligibility();

            var corrected = sut.CorrectEligibility(
                _testBed.Date, Money.Create(50_000m), 2, ["a", "b"], null, "Typo", _testBed.StaffUser);

            corrected.IncomeDocumentIds.Should().Equal("a", "b");
        }

        [Fact]
        public void GivenIncomeCorrectedToAnotherBand_WhenCorrected_ThenTheBandIsRecomputed()
        {
            var sut = _testBed.WithEligibility();

            var corrected = sut.CorrectEligibility(
                _testBed.Date, Money.Create(30_000m), 2, null, null, "Typo", _testBed.StaffUser);

            corrected.Should().BeEquivalentTo(new { IncomeBandOverPercentOfAmi = (int?)null, IncomeBandUpToPercentOfAmi = 50 });
        }

        [Fact]
        public void GivenIncomeOverTheLimit_WhenCorrected_ThenThrowsOverIncomeLimit()
        {
            var sut = _testBed.WithEligibility();

            var act = () => sut.CorrectEligibility(
                _testBed.Date, Money.Create(57_150.01m), 2, null, null, "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_eligibility.over_income_limit");
        }

        [Fact]
        public void GivenNothingChanged_WhenCorrected_ThenThrowsNoChange()
        {
            var sut = _testBed.WithEligibility();

            var act = () => sut.CorrectEligibility(
                _testBed.Date, Money.Create(40_000m), 2, ["income-doc"], null, "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_eligibility.no_change");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GivenNoReason_WhenCorrected_ThenThrowsReasonRequired(string? reason)
        {
            var sut = _testBed.WithEligibility();

            var act = () => sut.CorrectEligibility(
                _testBed.Date, Money.Create(50_000m), 2, null, null, reason, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_eligibility.reason_required");
        }

        [Fact]
        public void GivenEligibilityNotRecorded_WhenCorrected_ThenThrowsTrackNotRecorded()
        {
            var sut = _testBed.Fresh();

            var act = () => sut.CorrectEligibility(
                _testBed.Date, Money.Create(50_000m), 2, ["doc"], null, "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_eligibility.track_not_recorded");
        }

        [Fact]
        public void GivenTheBuyerWithdrew_WhenCorrected_ThenThrowsTerminalPhase()
        {
            var sut = _testBed.WithEligibility();
            sut = sut.Decide(a => a.WithdrawPurchase(_testBed.Date, "Changed mind", _testBed.StaffUser)).Aggregate;

            var act = () => sut.CorrectEligibility(
                _testBed.Date, Money.Create(50_000m), 2, null, null, "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>();
        }
    }

    public class TheCorrectAppraisalMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenARecordedAppraisal_WhenCorrected_ThenTheDetailsHoldTheNewValues()
        {
            var sut = _testBed.Fresh();
            sut = sut.Decide(a => a.RecordAppraisalCompleted(2_400_000m, _testBed.Date, "Jane", null, _testBed.StaffUser)).Aggregate;

            var corrected = sut.CorrectAppraisal(240_000m, _testBed.Date, "Jane", null, "Typo", _testBed.StaffUser);
            sut.Apply(corrected);

            corrected.Reason.Should().Be("Typo");
            sut.AppraisalDetails.Should().Be(new AppraisalDetails(240_000m, _testBed.Date, "Jane", null));
        }

        [Fact]
        public void GivenNothingChanged_WhenCorrected_ThenThrowsNoChange()
        {
            var sut = _testBed.Fresh();
            sut = sut.Decide(a => a.RecordAppraisalCompleted(300_000m, _testBed.Date, "Jane", null, _testBed.StaffUser)).Aggregate;

            var act = () => sut.CorrectAppraisal(300_000m, _testBed.Date, "Jane", "", "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_appraisal.no_change");
        }

        [Fact]
        public void GivenValueNotAboveZero_WhenCorrected_ThenThrowsAppraisedValueNonPositive()
        {
            var sut = _testBed.Fresh();
            sut = sut.Decide(a => a.RecordAppraisalCompleted(300_000m, _testBed.Date, "Jane", null, _testBed.StaffUser)).Aggregate;

            var act = () => sut.CorrectAppraisal(0m, _testBed.Date, "Jane", null, "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_appraisal.appraised_value_non_positive");
        }

        [Fact]
        public void GivenNoReason_WhenCorrected_ThenThrowsReasonRequired()
        {
            var sut = _testBed.Fresh();
            sut = sut.Decide(a => a.RecordAppraisalCompleted(300_000m, _testBed.Date, "Jane", null, _testBed.StaffUser)).Aggregate;

            var act = () => sut.CorrectAppraisal(310_000m, _testBed.Date, "Jane", null, " ", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_appraisal.reason_required");
        }

        [Fact]
        public void GivenAppraisalNotRecorded_WhenCorrected_ThenThrowsTrackNotRecorded()
        {
            var sut = _testBed.Fresh();

            var act = () => sut.CorrectAppraisal(310_000m, _testBed.Date, "Jane", null, "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_appraisal.track_not_recorded");
        }
    }

    public class TheCorrectFinancingMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenRecordedFinancing_WhenCorrected_ThenTheDetailsHoldTheNewValues()
        {
            var sut = _testBed.Fresh();
            sut = sut.Decide(a => a.RecordFinancingCommitted("First National", 2_400_000m, _testBed.Date, _testBed.StaffUser)).Aggregate;

            var corrected = sut.CorrectFinancing("First National", 240_000m, _testBed.Date, "Typo", _testBed.StaffUser);
            sut.Apply(corrected);

            sut.FinancingDetails.Should().Be(new FinancingDetails("First National", 240_000m, _testBed.Date));
        }

        [Fact]
        public void GivenNothingChanged_WhenCorrected_ThenThrowsNoChange()
        {
            var sut = _testBed.Fresh();
            sut = sut.Decide(a => a.RecordFinancingCommitted("First National", 240_000m, _testBed.Date, _testBed.StaffUser)).Aggregate;

            var act = () => sut.CorrectFinancing("First National", 240_000m, _testBed.Date, "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_financing.no_change");
        }

        [Fact]
        public void GivenNoReason_WhenCorrected_ThenThrowsReasonRequired()
        {
            var sut = _testBed.Fresh();
            sut = sut.Decide(a => a.RecordFinancingCommitted("First National", 240_000m, _testBed.Date, _testBed.StaffUser)).Aggregate;

            var act = () => sut.CorrectFinancing("First National", 250_000m, _testBed.Date, null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_financing.reason_required");
        }

        [Fact]
        public void GivenFinancingNotRecorded_WhenCorrected_ThenThrowsTrackNotRecorded()
        {
            var sut = _testBed.Fresh();

            var act = () => sut.CorrectFinancing("First National", 250_000m, _testBed.Date, "Typo", _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("homeownership.purchase_workflow.correct_financing.track_not_recorded");
        }
    }

    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();

        public DateOnly Date { get; } = new(2026, 9, 1);

        public PurchaseWorkflow Fresh() => Workflow();

        public PurchaseWorkflow WithEligibility()
        {
            var sut = Workflow();
            return sut.Decide(a => a.RecordEligibilityVerified(Date, Money.Create(40_000m), 2, ["income-doc"], null, StaffUser)).Aggregate;
        }

        private static IncomeLimitTable Schedule() => IncomeLimitTable.Create(
            new DateOnly(2026, 4, 1),
            [
                IncomeLimitLevel.Create(
                    50,
                    [.. new[] { 31_650m, 36_200m, 40_700m, 45_200m, 48_850m, 52_450m, 56_100m, 59_700m }.Select(Money.Create)]),
                IncomeLimitLevel.Create(
                    80,
                    [.. new[] { 50_000m, 57_150m, 64_300m, 71_400m, 77_150m, 82_850m, 88_550m, 94_250m }.Select(Money.Create)]),
            ]);

        private PurchaseWorkflow Workflow() =>
            EventReplay.Replay<PurchaseWorkflow>(PurchaseWorkflow.Initiate(
                PurchaseWorkflowId.Generate(),
                GroundLeaseTemplateId.Generate(),
                "land-parcel-1234",
                "home-5678",
                Homeowner.From("Alice Johnson", "alice@example.com", "+12025551234"),
                250_000m,
                StaffUser,
                "application-1",
                "household-1",
                Money.Create(55_000m),
                2,
                80,
                Schedule()));
    }
}
