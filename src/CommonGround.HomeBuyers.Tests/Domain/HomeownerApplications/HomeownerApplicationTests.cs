using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeownerApplications;

public class HomeownerApplicationTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();
        public DateOnly SelectedDate { get; } = new(2026, 7, 20);
        public Household Household { get; }

        public TestBed()
        {
            Household = Household.Register(
                [
                    HouseholdMember.Create("Alice Johnson", "alice@example.com", "555-0100", true),
                    HouseholdMember.Create("Bob Johnson", "bob@example.com", "555-0101", false),
                ],
                HouseholdCircumstances.Create(false, 65000m),
                StaffUser);
        }

        public TestBed WithReadiness(bool counseling = true, bool orientation = true, DateOnly? prequalificationExpiry = null)
        {
            if (counseling)
                Household.RecordHousingCounseling("CERT-001", new DateOnly(2026, 6, 2), "Counseling Org", StaffUser);
            if (orientation)
                Household.RecordCltOrientation(new DateOnly(2026, 6, 3), StaffUser);
            Household.RecordMortgagePrequalification("Lender", 210000m, prequalificationExpiry ?? new DateOnly(2027, 1, 1), StaffUser);
            return this;
        }

        public HomeownerApplication SubmittedApplication() =>
            HomeownerApplication.Submit(Household.Id, Household.Declare(), null, StaffUser);

        public HomeownerApplication ApprovedApplication()
        {
            var application = SubmittedApplication();
            application.Approve(new DateOnly(2026, 6, 1), null, StaffUser);
            return application;
        }

        public HomeownerApplication SelectedApplication()
        {
            WithReadiness();
            var application = ApprovedApplication();
            application.SelectForHome(Household, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, SelectedDate, StaffUser);
            return application;
        }
    }

    public class TheSubmitMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHouseholdHasOpenApplication_WhenSubmitted_ThenThrowsHouseholdHasOpenApplication()
        {
            var act = () => HomeownerApplication.Submit(
                _testBed.Household.Id, _testBed.Household.Declare(), openApplicationId: HomeownerApplicationId.Generate(), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.submit.household_has_open_application");
        }

        [Fact]
        public void GivenNoOpenApplication_WhenSubmitted_ThenIsSubmittedWithHouseholdSnapshot()
        {
            var sut = HomeownerApplication.Submit(_testBed.Household.Id, _testBed.Household.Declare(), null, _testBed.StaffUser);

            sut.Phase.Should().Be(ApplicationPhase.Submitted);
            sut.Declaration.Should().Be(_testBed.Household.Declare());
        }
    }

    public class TheApproveMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenAlreadyApproved_WhenApproved_ThenThrowsPhaseNotSubmitted()
        {
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.Approve(new DateOnly(2026, 6, 2), null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.approve.phase_not_submitted");
        }
    }

    public class TheDenyApplicationMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GivenBlankReason_WhenDenied_ThenThrowsReasonRequired(string reason)
        {
            var sut = _testBed.SubmittedApplication();

            var act = () => sut.DenyApplication(reason, new DateOnly(2026, 6, 1), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.deny.reason_required");
        }
    }

    public class TheWithdrawMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenWithdrawnApplication_WhenWithdrawnAgain_ThenThrowsTerminalPhase()
        {
            var sut = _testBed.SubmittedApplication();
            sut.Withdraw(new DateOnly(2026, 6, 1), null, _testBed.StaffUser);

            var act = () => sut.Withdraw(new DateOnly(2026, 6, 2), null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.modify.terminal_phase");
        }
    }

    public class TheSelectForHomeMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenSubmittedApplication_WhenSelected_ThenThrowsPhaseNotApproved()
        {
            _testBed.WithReadiness();
            var sut = _testBed.SubmittedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.phase_not_approved");
        }

        [Fact]
        public void GivenNonPositivePurchasePrice_WhenSelected_ThenThrowsPurchasePriceNonPositive()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, "home-1", "land-parcel-1", "ground-lease-template-1", purchasePrice: 0m, _testBed.SelectedDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.purchase_price_non_positive");
        }

        [Fact]
        public void GivenIncompleteReadiness_WhenSelected_ThenThrowsHouseholdReadinessIncompleteListingOutstandingItems()
        {
            _testBed.WithReadiness(orientation: false);
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser);

            var thrown = act.Should().Throw<BusinessRuleViolationException>().Which;
            thrown.Code.Should().Be("home_buyers.homeowner_application.select_applicant.household_readiness_incomplete");
            thrown.Args["outstandingItems"].Should().BeEquivalentTo(new[] { "CltOrientation" });
        }

        [Fact]
        public void GivenPrequalificationExpiredBeforeSelectedDate_WhenSelected_ThenThrowsPrequalificationExpired()
        {
            _testBed.WithReadiness(prequalificationExpiry: _testBed.SelectedDate.AddDays(-1));
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.prequalification_expired");
        }

        [Fact]
        public void GivenAnotherHousehold_WhenSelected_ThenThrowsArgumentException()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();
            var otherHousehold = new TestBed().WithReadiness().Household;

            var act = () => sut.SelectForHome(
                otherHousehold, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenReadyHousehold_WhenSelected_ThenIsSelectedWithPrimaryMemberAsApplicant()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            sut.SelectForHome(
                _testBed.Household, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser);

            sut.Phase.Should().Be(ApplicationPhase.Selected);
            sut.UncommittedEvents.OfType<ApplicationSelectedForHome>().Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new { ApplicantFullName = "Alice Johnson", ApplicantEmail = "alice@example.com" });
        }
    }

    public class TheCanBeSelectedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenApprovedApplicationWithReadyHousehold_WhenChecked_ThenIsTrue()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            sut.CanBeSelected(_testBed.Household, _testBed.SelectedDate).Should().BeTrue();
        }

        [Fact]
        public void GivenSubmittedApplication_WhenChecked_ThenIsFalse()
        {
            _testBed.WithReadiness();
            var sut = _testBed.SubmittedApplication();

            sut.CanBeSelected(_testBed.Household, _testBed.SelectedDate).Should().BeFalse();
        }

        [Fact]
        public void GivenIncompleteReadiness_WhenChecked_ThenIsFalse()
        {
            _testBed.WithReadiness(counseling: false);
            var sut = _testBed.ApprovedApplication();

            sut.CanBeSelected(_testBed.Household, _testBed.SelectedDate).Should().BeFalse();
        }

        [Fact]
        public void GivenPrequalificationExpiredBeforeSelectedDate_WhenChecked_ThenIsFalse()
        {
            _testBed.WithReadiness(prequalificationExpiry: _testBed.SelectedDate.AddDays(-1));
            var sut = _testBed.ApprovedApplication();

            sut.CanBeSelected(_testBed.Household, _testBed.SelectedDate).Should().BeFalse();
        }

        [Fact]
        public void GivenAnotherHousehold_WhenChecked_ThenIsFalse()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();
            var otherHousehold = new TestBed().WithReadiness().Household;

            sut.CanBeSelected(otherHousehold, _testBed.SelectedDate).Should().BeFalse();
        }
    }

    public class ThePassOverMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenSubmittedApplication_WhenPassedOver_ThenThrowsPhaseNotApproved()
        {
            var sut = _testBed.SubmittedApplication();

            var act = () => sut.PassOver(HomeSelectionId.Generate(), "home-1");

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.pass_over.phase_not_approved");
        }

        [Fact]
        public void GivenApprovedApplication_WhenPassedOver_ThenStaysApprovedAndRecordsTheHousehold()
        {
            var sut = _testBed.ApprovedApplication();

            sut.PassOver(HomeSelectionId.Generate(), "home-1");

            sut.Phase.Should().Be(ApplicationPhase.Approved);
            sut.UncommittedEvents.OfType<ApplicationPassedOver>().Should().ContainSingle()
                .Which.HouseholdId.Should().Be(_testBed.Household.Id.Value);
        }
    }

    public class TheReturnToWaitingListMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenApprovedApplication_WhenReturned_ThenThrowsPhaseNotSelected()
        {
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.ReturnToWaitingList("home-1", "Financing fell through", _testBed.SelectedDate);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.return_to_waiting_list.phase_not_selected");
        }

        [Fact]
        public void GivenSelectedApplication_WhenReturned_ThenIsApprovedAgainAndCanBeSelectedAgain()
        {
            var sut = _testBed.SelectedApplication();

            sut.ReturnToWaitingList("home-1", "Financing fell through", _testBed.SelectedDate.AddDays(30));

            sut.Phase.Should().Be(ApplicationPhase.Approved);
            sut.CanBeSelected(_testBed.Household, _testBed.SelectedDate.AddDays(31)).Should().BeTrue();
        }
    }

    public class TheRecordPurchaseClosedMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenApprovedApplication_WhenPurchaseClosed_ThenThrowsPhaseNotSelected()
        {
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.RecordPurchaseClosed("home-1", _testBed.SelectedDate);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.record_purchase_closed.phase_not_selected");
        }

        [Fact]
        public void GivenSelectedApplication_WhenPurchaseClosed_ThenIsPurchasedAndTerminal()
        {
            var sut = _testBed.SelectedApplication();

            sut.RecordPurchaseClosed("home-1", _testBed.SelectedDate.AddDays(60));

            sut.Phase.Should().Be(ApplicationPhase.Purchased);
            sut.IsTerminal.Should().BeTrue();
        }
    }
}
