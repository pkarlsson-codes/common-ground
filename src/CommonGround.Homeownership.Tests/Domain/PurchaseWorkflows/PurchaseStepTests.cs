using CommonGround.Homeownership.Domain.PurchaseWorkflows;
using FluentAssertions;

namespace CommonGround.Homeownership.Tests.Domain.PurchaseWorkflows;

public class PurchaseStepTests
{
    public class TheWaitingOnMethod
    {
        [Fact]
        public void GivenNothingRecorded_WhenAsked_ThenWaitsOnEligibility()
        {
            var step = PurchaseStep.WaitingOn(false, false, false, false, false, false, false);

            step.Should().Be(PurchaseStep.Eligibility);
        }

        [Fact]
        public void GivenOnlyEligibilityRecorded_WhenAsked_ThenWaitsOnAppraisal()
        {
            var step = PurchaseStep.WaitingOn(true, false, false, false, false, false, false);

            step.Should().Be(PurchaseStep.Appraisal);
        }

        [Fact]
        public void GivenEligibilityAppraisalAndFinancingRecorded_WhenAsked_ThenWaitsOnInspection()
        {
            var step = PurchaseStep.WaitingOn(true, true, true, false, false, false, false);

            step.Should().Be(PurchaseStep.Inspection);
        }

        [Fact]
        public void GivenAllButTheAppraisalRecorded_WhenAsked_ThenWaitsOnAppraisal()
        {
            var step = PurchaseStep.WaitingOn(true, false, true, true, true, true, false);

            step.Should().Be(PurchaseStep.Appraisal);
        }

        [Fact]
        public void GivenLaterStepsRecordedFirst_WhenAsked_ThenWaitsOnTheEarliestStepNotRecorded()
        {
            var step = PurchaseStep.WaitingOn(true, true, false, true, true, true, false);

            step.Should().Be(PurchaseStep.Financing);
        }

        [Fact]
        public void GivenAllButTheLeaseRecorded_WhenAsked_ThenWaitsOnTheLeaseSigning()
        {
            var step = PurchaseStep.WaitingOn(true, true, true, true, true, false, false);

            step.Should().Be(PurchaseStep.LeaseSigning);
        }

        [Fact]
        public void GivenSixTracksRecordedButNotDisclosure_WhenAsked_ThenWaitsOnDisclosure()
        {
            var step = PurchaseStep.WaitingOn(true, true, true, true, true, true, false);

            step.Should().Be(PurchaseStep.Disclosure);
        }

        [Fact]
        public void GivenEveryStepRecorded_WhenAsked_ThenWaitsOnClosing()
        {
            var step = PurchaseStep.WaitingOn(true, true, true, true, true, true, true);

            step.Should().Be(PurchaseStep.Closing);
        }
    }
}
