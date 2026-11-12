using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.HomeownerApplications;

public class HomeownerApplicationTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();
        public DateOnly SelectedDate { get; } = new(2026, 7, 20);
        public HomeSelectionId SelectionId { get; } = HomeSelectionId.Generate();
        public Household Household { get; private set; }

        public TestBed()
        {
            Household = EventReplay.Replay<Household>(Household.Register(
                HouseholdId.Generate(),
                [
                    HouseholdMember.Create(PersonName.Create("Alice Johnson"), EmailAddress.Create("alice@example.com"), new PhoneNumber("(202) 555-0100"), true),
                    HouseholdMember.Create(PersonName.Create("Bob Johnson"), EmailAddress.Create("bob@example.com"), new PhoneNumber("(202) 555-0101"), false),
                ],
                HouseholdCircumstances.Create(false, Money.Create(65000m)),
                StaffUser));
        }

        public SelectionCandidate Candidate(HomeownerApplication application) =>
            SelectionCandidate.Create(application.Id, Household.Id, Money.Create(55_000m), 2, false, new DateOnly(2026, 6, 1), incomeLimit: null);

        public TestBed WithReadiness(bool counseling = true, bool orientation = true, DateOnly? prequalificationExpiry = null)
        {
            if (counseling)
                Household = Household.Decide(a => a.RecordHousingCounseling("CERT-001", new DateOnly(2026, 6, 2), "Counseling Org", StaffUser)).Aggregate;
            if (orientation)
                Household = Household.Decide(a => a.RecordCltOrientation(new DateOnly(2026, 6, 3), StaffUser)).Aggregate;
            Household = Household.Decide(a => a.RecordMortgagePrequalification("Lender", 210000m, prequalificationExpiry ?? new DateOnly(2027, 1, 1), StaffUser)).Aggregate;
            return this;
        }

        public HomeownerApplication SubmittedApplication() =>
            EventReplay.Replay<HomeownerApplication>(HomeownerApplication.Submit(HomeownerApplicationId.Generate(), Household.Id, Household.Declare(), null, StaffUser));

        public HomeownerApplication ApprovedApplication()
        {
            var application = SubmittedApplication();
            application = application.Decide(a => a.Approve(new DateOnly(2026, 6, 1), null, StaffUser)).Aggregate;
            return application;
        }

        public HomeownerApplication SelectedApplication()
        {
            WithReadiness();
            var application = ApprovedApplication();
            application = application.Decide(a => a.SelectForHome(Household, SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, SelectedDate, StaffUser, Candidate(a), null, null)).Aggregate;
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
                HomeownerApplicationId.Generate(),
                _testBed.Household.Id, _testBed.Household.Declare(), openApplicationId: HomeownerApplicationId.Generate(), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.submit.household_has_open_application");
        }

        [Fact]
        public void GivenNoOpenApplication_WhenSubmitted_ThenIsSubmittedWithHouseholdSnapshot()
        {
            var sut = EventReplay.Replay<HomeownerApplication>(HomeownerApplication.Submit(HomeownerApplicationId.Generate(), _testBed.Household.Id, _testBed.Household.Declare(), null, _testBed.StaffUser));

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
            sut = sut.Decide(a => a.Withdraw(new DateOnly(2026, 6, 1), null, _testBed.StaffUser)).Aggregate;

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
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, _testBed.Candidate(sut), null, null);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.phase_not_approved");
        }

        [Fact]
        public void GivenNonPositivePurchasePrice_WhenSelected_ThenThrowsPurchasePriceNonPositive()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", purchasePrice: 0m, _testBed.SelectedDate, _testBed.StaffUser, _testBed.Candidate(sut), null, null);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.purchase_price_non_positive");
        }

        [Fact]
        public void GivenIncompleteReadiness_WhenSelected_ThenThrowsHouseholdReadinessIncompleteListingOutstandingItems()
        {
            _testBed.WithReadiness(orientation: false);
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, _testBed.Candidate(sut), null, null);

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
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, _testBed.Candidate(sut), null, null);

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
                otherHousehold, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, _testBed.Candidate(sut), null, null);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenReadyHousehold_WhenSelected_ThenIsSelectedWithPrimaryMemberAsApplicant()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            (sut, var events) = sut.Decide(a => a.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, _testBed.Candidate(sut), null, null));

            sut.Phase.Should().Be(ApplicationPhase.Selected);
            events.OfType<ApplicationSelectedForHome>().Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new
                {
                    HomeSelectionId = _testBed.SelectionId.Value,
                    ApplicantFullName = "Alice Johnson",
                    ApplicantEmail = "alice@example.com",
                });
        }

        [Fact]
        public void GivenTheSelectionJudgedByALimit_WhenSelected_ThenCarriesTheLimitAndTheIncomeItJudged()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();
            var level = IncomeLimitLevel.Create(
                80, [.. new[] { 50_000m, 57_150m, 64_300m, 71_400m, 77_150m, 82_850m, 88_550m, 94_250m }.Select(Money.Create)]);

            var selected = sut.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m,
                _testBed.SelectedDate, _testBed.StaffUser, _testBed.Candidate(sut), level, new DateOnly(2026, 4, 1));

            selected.Should().BeEquivalentTo(new
            {
                AnnualIncome = Money.Create(55_000m),
                HouseholdSize = 2,
                IncomeLimitPercentOfAmi = 80,
                IncomeLimitsEffectiveFrom = new DateOnly(2026, 4, 1),
                IncomeLimits = level.Limits,
            });
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
        public void GivenApplicationThatCouldNotHaveBeenSelected_WhenPassedOver_ThenDecidesNothing()
        {
            _testBed.WithReadiness();
            var sut = _testBed.SubmittedApplication();

            var passedOver = sut.PassOver(_testBed.SelectionId, "home-1", _testBed.Household, _testBed.SelectedDate);

            passedOver.Should().BeNull();
        }

        [Fact]
        public void GivenApprovedApplication_WhenPassedOver_ThenStaysApprovedAndRecordsTheHousehold()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            (sut, var events) = sut.Decide(a => a.PassOver(_testBed.SelectionId, "home-1", _testBed.Household, _testBed.SelectedDate));

            sut.Phase.Should().Be(ApplicationPhase.Approved);
            events.OfType<ApplicationPassedOver>().Should().ContainSingle()
                .Which.HouseholdId.Should().Be(_testBed.Household.Id.Value);
        }

        [Fact]
        public void GivenApplicationAlreadyPassedOverInTheSelection_WhenPassedOverAgain_ThenDecidesNothing()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();
            sut = sut.Decide(a => a.PassOver(_testBed.SelectionId, "home-1", _testBed.Household, _testBed.SelectedDate)).Aggregate;

            var passedOver = sut.PassOver(_testBed.SelectionId, "home-1", _testBed.Household, _testBed.SelectedDate);

            passedOver.Should().BeNull();
        }

        [Fact]
        public void GivenApplicationPassedOverInAnotherSelection_WhenPassedOver_ThenRecordsThisSelection()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();
            sut = sut.Decide(a => a.PassOver(HomeSelectionId.Generate(), "home-2", _testBed.Household, _testBed.SelectedDate)).Aggregate;

            var passedOver = sut.PassOver(_testBed.SelectionId, "home-1", _testBed.Household, _testBed.SelectedDate);

            passedOver.Should().NotBeNull()
                .And.Subject.As<ApplicationPassedOver>().SelectionId.Should().Be(_testBed.SelectionId.Value);
        }
    }

    public class TheReturnToWaitingListMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenSubmittedApplication_WhenReturned_ThenThrowsPhaseNotSelected()
        {
            var sut = _testBed.SubmittedApplication();

            var act = () => sut.ReturnToWaitingList("home-1", "Financing fell through", _testBed.SelectedDate);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.return_to_waiting_list.phase_not_selected");
        }

        [Fact]
        public void GivenApplicationAlreadyReturned_WhenReturned_ThenDecidesNothing()
        {
            var sut = _testBed.SelectedApplication();
            sut = sut.Decide(a => a.ReturnToWaitingList("home-1", "Financing fell through", _testBed.SelectedDate.AddDays(30))).Aggregate;

            var returned = sut.ReturnToWaitingList("home-1", "Financing fell through", _testBed.SelectedDate.AddDays(30));

            returned.Should().BeNull();
        }

        [Fact]
        public void GivenSelectedApplication_WhenReturned_ThenIsApprovedAgainAndCanBeSelectedAgain()
        {
            var sut = _testBed.SelectedApplication();

            sut = sut.Decide(a => a.ReturnToWaitingList("home-1", "Financing fell through", _testBed.SelectedDate.AddDays(30))).Aggregate;

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
        public void GivenPurchaseAlreadyRecorded_WhenPurchaseClosed_ThenDecidesNothing()
        {
            var sut = _testBed.SelectedApplication();
            sut = sut.Decide(a => a.RecordPurchaseClosed("home-1", _testBed.SelectedDate.AddDays(60))).Aggregate;

            var closed = sut.RecordPurchaseClosed("home-1", _testBed.SelectedDate.AddDays(60));

            closed.Should().BeNull();
        }

        [Fact]
        public void GivenSelectedApplication_WhenPurchaseClosed_ThenIsPurchasedAndTerminal()
        {
            var sut = _testBed.SelectedApplication();

            sut = sut.Decide(a => a.RecordPurchaseClosed("home-1", _testBed.SelectedDate.AddDays(60))).Aggregate;

            sut.Phase.Should().Be(ApplicationPhase.Purchased);
            sut.IsTerminal.Should().BeTrue();
        }
    }
}
