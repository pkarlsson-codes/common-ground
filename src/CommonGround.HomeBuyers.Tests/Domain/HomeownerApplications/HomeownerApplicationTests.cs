using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
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

        public HomeownerApplication Submit(HomeownerApplicationId? openApplicationId = null) =>
            HomeownerApplication.Submit(Household.Id, Household.Declare(), openApplicationId, StaffUser);

        public HomeownerApplication SubmitApproved()
        {
            var application = Submit();
            application.Approve(new DateOnly(2026, 6, 1), null, StaffUser);
            return application;
        }

        public void SelectForHome(HomeownerApplication application, Household? household = null, decimal purchasePrice = 200000m) =>
            application.SelectForHome(
                household ?? Household, "home-1", "land-parcel-1", "ground-lease-template-1",
                purchasePrice, SelectedDate, StaffUser);
    }

    public class TheSubmitMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenHouseholdHasOpenApplication_WhenSubmitted_ThenThrowsHouseholdHasOpenApplication()
        {
            var act = () => _testBed.Submit(openApplicationId: HomeownerApplicationId.Generate());

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.submit.household_has_open_application");
        }

        [Fact]
        public void GivenNoOpenApplication_WhenSubmitted_ThenIsSubmittedWithHouseholdSnapshot()
        {
            var sut = _testBed.Submit();

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
            var sut = _testBed.SubmitApproved();

            var act = () => sut.Approve(new DateOnly(2026, 6, 2), null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.approve.phase_not_submitted");
        }
    }

    public class TheWithdrawMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenWithdrawnApplication_WhenWithdrawnAgain_ThenThrowsTerminalPhase()
        {
            var sut = _testBed.Submit();
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
            var sut = _testBed.Submit();

            var act = () => _testBed.SelectForHome(sut);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.phase_not_approved");
        }

        [Fact]
        public void GivenNonPositivePurchasePrice_WhenSelected_ThenThrowsPurchasePriceNonPositive()
        {
            _testBed.WithReadiness();
            var sut = _testBed.SubmitApproved();

            var act = () => _testBed.SelectForHome(sut, purchasePrice: 0m);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.purchase_price_non_positive");
        }

        [Fact]
        public void GivenIncompleteReadiness_WhenSelected_ThenThrowsHouseholdReadinessIncompleteListingOutstandingItems()
        {
            _testBed.WithReadiness(orientation: false);
            var sut = _testBed.SubmitApproved();

            var act = () => _testBed.SelectForHome(sut);

            var thrown = act.Should().Throw<BusinessRuleViolationException>().Which;
            thrown.Code.Should().Be("home_buyers.homeowner_application.select_applicant.household_readiness_incomplete");
            thrown.Args["outstandingItems"].Should().BeEquivalentTo(new[] { "CltOrientation" });
        }

        [Fact]
        public void GivenPrequalificationExpiredBeforeSelectedDate_WhenSelected_ThenThrowsPrequalificationExpired()
        {
            _testBed.WithReadiness(prequalificationExpiry: _testBed.SelectedDate.AddDays(-1));
            var sut = _testBed.SubmitApproved();

            var act = () => _testBed.SelectForHome(sut);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.prequalification_expired");
        }

        [Fact]
        public void GivenAnotherHousehold_WhenSelected_ThenThrowsArgumentException()
        {
            _testBed.WithReadiness();
            var sut = _testBed.SubmitApproved();
            var otherHousehold = new TestBed().WithReadiness().Household;

            var act = () => _testBed.SelectForHome(sut, otherHousehold);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenReadyHousehold_WhenSelected_ThenIsSelectedWithPrimaryMemberAsApplicant()
        {
            _testBed.WithReadiness();
            var sut = _testBed.SubmitApproved();

            _testBed.SelectForHome(sut);

            sut.Phase.Should().Be(ApplicationPhase.Selected);
            sut.UncommittedEvents.OfType<ApplicationSelectedForHome>().Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new { ApplicantFullName = "Alice Johnson", ApplicantEmail = "alice@example.com" });
        }
    }
}
