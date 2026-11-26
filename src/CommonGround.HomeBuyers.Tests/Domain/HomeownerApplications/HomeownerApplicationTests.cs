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

        public TestBed WithReadiness(bool counseling = true, bool orientation = true, DateOnly? prequalificationExpiry = null)
        {
            if (counseling)
                Household = Household.Decide(a => a.RecordHousingCounseling("CERT-001", new DateOnly(2026, 6, 2), "Counseling Org", new DateOnly(2026, 6, 10), StaffUser)).Aggregate;
            if (orientation)
                Household = Household.Decide(a => a.RecordCltOrientation(new DateOnly(2026, 6, 3), new DateOnly(2026, 6, 10), StaffUser)).Aggregate;
            Household = Household.Decide(a => a.RecordMortgagePrequalification("Lender", 210000m, prequalificationExpiry ?? new DateOnly(2027, 1, 1), StaffUser)).Aggregate;
            return this;
        }

        public TestBed WithPrequalificationExpiryRecorded(DateOnly expiry)
        {
            Household = Household.Decide(a => a.ExpirePrequalification(expiry)).Aggregate;
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
            application = application.Decide(a => a.SelectForHome(Household, SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, SelectedDate, StaffUser, null, null)).Aggregate;
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

        [Fact]
        public void GivenApprovedApplication_WhenDenied_ThenThrowsPhaseNotSubmitted()
        {
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.DenyApplication("Moved away", new DateOnly(2026, 7, 1), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.deny.phase_not_submitted");
        }
    }

    public class TheRemoveFromWaitingListMethod
    {
        private static readonly DateOnly Today = new(2026, 9, 1);
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenSubmittedApplication_WhenRemoved_ThenThrowsPhaseNotApproved()
        {
            var sut = _testBed.SubmittedApplication();

            var act = () => sut.RemoveFromWaitingList(WaitingListRemovalReason.NoResponse, null, Today, Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.remove_from_waiting_list.phase_not_approved");
        }

        [Fact]
        public void GivenSelectedApplication_WhenRemoved_ThenThrowsSelectedForPurchase()
        {
            var sut = _testBed.SelectedApplication();

            var act = () => sut.RemoveFromWaitingList(WaitingListRemovalReason.NoResponse, null, Today, Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.modify.selected_for_purchase");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("  ")]
        public void GivenOtherWithoutNote_WhenRemoved_ThenThrowsNoteRequired(string? note)
        {
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.RemoveFromWaitingList(WaitingListRemovalReason.Other, note, Today, Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.remove_from_waiting_list.note_required");
        }

        [Fact]
        public void GivenDateAfterToday_WhenRemoved_ThenThrowsRemovedDateInFuture()
        {
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.RemoveFromWaitingList(WaitingListRemovalReason.NoResponse, null, Today.AddDays(1), Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.remove_from_waiting_list.removed_date_in_future");
        }

        [Fact]
        public void GivenDateBeforeApproval_WhenRemoved_ThenThrowsRemovedDateBeforeApproval()
        {
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.RemoveFromWaitingList(WaitingListRemovalReason.NoResponse, null, sut.ApprovedDate!.Value.AddDays(-1), Today, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.remove_from_waiting_list.removed_date_before_approval");
        }

        [Fact]
        public void GivenApprovedApplication_WhenRemovedOnTheApprovedDate_ThenIsRemovedAndTerminal()
        {
            var sut = _testBed.ApprovedApplication();

            var (removed, events) = sut.Decide(a => a.RemoveFromWaitingList(
                WaitingListRemovalReason.NoResponse, "  Three calls and a letter  ", sut.ApprovedDate!.Value, Today, _testBed.StaffUser));

            removed.Phase.Should().Be(ApplicationPhase.Removed);
            removed.IsTerminal.Should().BeTrue();
            events.Should().ContainSingle().Which.Should().BeOfType<ApplicationRemovedFromWaitingList>()
                .Which.Should().Match<ApplicationRemovedFromWaitingList>(e =>
                    e.Reason == WaitingListRemovalReason.NoResponse && e.Note == "Three calls and a letter" && e.RemovedDate == sut.ApprovedDate);
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

        [Fact]
        public void GivenSelectedApplication_WhenWithdrawn_ThenThrowsSelectedForPurchase()
        {
            var sut = _testBed.SelectedApplication();

            var act = () => sut.Withdraw(_testBed.SelectedDate.AddDays(1), null, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.modify.selected_for_purchase");
        }

        [Fact]
        public void GivenSelectedApplication_WhenDenied_ThenThrowsSelectedForPurchase()
        {
            var sut = _testBed.SelectedApplication();

            var act = () => sut.DenyApplication("No longer eligible", _testBed.SelectedDate.AddDays(1), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.modify.selected_for_purchase");
        }
    }

    public class TheIsTerminalProperty
    {
        private readonly TestBed _testBed = new();

        // Selected counts as open, so a household in a purchase can't submit a second application that
        // would sit beside the first once the purchase falls through and the first returns to Approved.
        [Fact]
        public void GivenSelectedApplication_ThenIsNotTerminal()
        {
            var sut = _testBed.SelectedApplication();

            sut.IsTerminal.Should().BeFalse();
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
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, null, null);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.phase_not_approved");
        }

        [Fact]
        public void GivenNonPositivePurchasePrice_WhenSelected_ThenThrowsPurchasePriceNonPositive()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", purchasePrice: 0m, _testBed.SelectedDate, _testBed.StaffUser, null, null);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.purchase_price_non_positive");
        }

        [Fact]
        public void GivenIncompleteReadiness_WhenSelected_ThenThrowsHouseholdReadinessIncompleteListingOutstandingItems()
        {
            _testBed.WithReadiness(orientation: false);
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, null, null);

            var thrown = act.Should().Throw<BusinessRuleViolationException>().Which;
            thrown.Code.Should().Be("home_buyers.homeowner_application.select_applicant.household_readiness_incomplete");
            thrown.Args["outstandingItems"].Should().BeEquivalentTo(new[] { "CltOrientation" });
        }

        [Fact]
        public void GivenExpiryRecorded_WhenSelectedAfterIt_ThenThrowsPrequalificationExpired()
        {
            var expiry = _testBed.SelectedDate.AddDays(-1);
            _testBed.WithReadiness(prequalificationExpiry: expiry);
            _testBed.WithPrequalificationExpiryRecorded(expiry);
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, null, null);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.homeowner_application.select_applicant.prequalification_expired");
        }

        [Fact]
        public void GivenPrequalificationExpiredBeforeSelectedDate_WhenSelected_ThenThrowsPrequalificationExpired()
        {
            _testBed.WithReadiness(prequalificationExpiry: _testBed.SelectedDate.AddDays(-1));
            var sut = _testBed.ApprovedApplication();

            var act = () => sut.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, null, null);

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
                otherHousehold, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, null, null);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GivenReadyHousehold_WhenSelected_ThenIsSelectedWithPrimaryMemberAsApplicant()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            (sut, var events) = sut.Decide(a => a.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m, _testBed.SelectedDate, _testBed.StaffUser, null, null));

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
        public void GivenTheSelectionJudgedByALimit_WhenSelected_ThenCarriesTheLimitAndTheIncomeDeclaredOnTheApplication()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();
            var level = IncomeLimitLevel.Create(
                80, [.. new[] { 50_000m, 57_150m, 64_300m, 71_400m, 77_150m, 82_850m, 88_550m, 94_250m }.Select(Money.Create)]);
            var limits = IncomeLimitTable.Create(new DateOnly(2026, 4, 1), [level]);

            var selected = sut.SelectForHome(
                _testBed.Household, _testBed.SelectionId, "home-1", "land-parcel-1", "ground-lease-template-1", 200000m,
                _testBed.SelectedDate, _testBed.StaffUser, 80, limits);

            selected.Should().BeEquivalentTo(new
            {
                AnnualIncome = Money.Create(65_000m),
                HouseholdSize = 2,
                IncomeLimitPercentOfAmi = 80,
                IncomeLimitsEffectiveFrom = new DateOnly(2026, 4, 1),
                IncomeLimitLevels = limits.ToEntries(),
            });
        }
    }

    public class ThePassOverMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenApprovedApplication_WhenPassedOver_ThenStaysApprovedAndRecordsTheHousehold()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();

            (sut, var events) = sut.Decide(a => a.PassOver(_testBed.SelectionId, "home-1"));

            sut.Phase.Should().Be(ApplicationPhase.Approved);
            events.OfType<ApplicationPassedOver>().Should().ContainSingle()
                .Which.HouseholdId.Should().Be(_testBed.Household.Id.Value);
        }

        [Fact]
        public void GivenApplicationAlreadyPassedOverInTheSelection_WhenPassedOverAgain_ThenDecidesNothing()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();
            sut = sut.Decide(a => a.PassOver(_testBed.SelectionId, "home-1")).Aggregate;

            var passedOver = sut.PassOver(_testBed.SelectionId, "home-1");

            passedOver.Should().BeNull();
        }

        [Fact]
        public void GivenApplicationPassedOverInAnotherSelection_WhenPassedOver_ThenRecordsThisSelection()
        {
            _testBed.WithReadiness();
            var sut = _testBed.ApprovedApplication();
            sut = sut.Decide(a => a.PassOver(HomeSelectionId.Generate(), "home-2")).Aggregate;

            var passedOver = sut.PassOver(_testBed.SelectionId, "home-1");

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
        public void GivenSelectedApplication_WhenReturned_ThenIsApprovedAgainKeepingItsApprovalDate()
        {
            var sut = _testBed.SelectedApplication();

            sut = sut.Decide(a => a.ReturnToWaitingList("home-1", "Financing fell through", _testBed.SelectedDate.AddDays(30))).Aggregate;

            sut.Phase.Should().Be(ApplicationPhase.Approved);
            sut.ApprovedDate.Should().Be(new DateOnly(2026, 6, 1));
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
