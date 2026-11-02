using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Exceptions;
using CommonGround.Testing;
using FluentAssertions;

namespace CommonGround.HomeBuyers.Tests.Domain.Households;

public class HouseholdTests
{
    public class TestBed
    {
        public UserId StaffUser { get; } = UserId.Generate();
        public HouseholdCircumstances Circumstances { get; } = HouseholdCircumstances.Create(false, 65000m);

        public HouseholdMember Primary { get; } = HouseholdMember.Create(PersonName.Create("Alice Johnson"), EmailAddress.Create("alice@example.com"), new PhoneNumber("(202) 555-0100"), true);
        public HouseholdMember Partner { get; } = HouseholdMember.Create(PersonName.Create("Bob Johnson"), EmailAddress.Create("bob@example.com"), new PhoneNumber("(202) 555-0101"), false);

        public Household RegisteredHousehold() => EventReplay.Replay<Household>(Household.Register(HouseholdId.Generate(), [Primary, Partner], Circumstances, StaffUser));

        public Household ReadyHousehold(DateOnly prequalificationExpiry)
        {
            var household = RegisteredHousehold();
            household = household.Decide(a => a.RecordHousingCounseling("CERT-001", new DateOnly(2026, 6, 2), "Counseling Org", StaffUser)).Aggregate;
            household = household.Decide(a => a.RecordCltOrientation(new DateOnly(2026, 6, 3), StaffUser)).Aggregate;
            household = household.Decide(a => a.RecordMortgagePrequalification("Lender", 210000m, prequalificationExpiry, StaffUser)).Aggregate;
            return household;
        }
    }

    public class TheRegisterMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNoPrimaryMember_WhenRegistered_ThenThrowsMustHaveExactlyOnePrimaryMember()
        {
            var act = () => Household.Register(HouseholdId.Generate(), [_testBed.Partner], _testBed.Circumstances, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.household.register.must_have_exactly_one_primary_member");
        }

        [Fact]
        public void GivenTwoPrimaryMembers_WhenRegistered_ThenThrowsMustHaveExactlyOnePrimaryMember()
        {
            var secondPrimary = HouseholdMember.Create(PersonName.Create("Carol Smith"), EmailAddress.Create("carol@example.com"), new PhoneNumber("(202) 555-0102"), true);

            var act = () => Household.Register(HouseholdId.Generate(), [_testBed.Primary, secondPrimary], _testBed.Circumstances, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.household.register.must_have_exactly_one_primary_member");
        }

        [Fact]
        public void GivenValidMembers_WhenRegistered_ThenKeepsEachMembersId()
        {
            var registered = Household.Register(HouseholdId.Generate(), [_testBed.Primary, _testBed.Partner], _testBed.Circumstances, _testBed.StaffUser);

            EventReplay.Replay<Household>(registered).Members.Select(m => m.Id).Should().Equal(_testBed.Primary.Id, _testBed.Partner.Id);
            registered.Members
                .Select(m => m.MemberId).Should().Equal(_testBed.Primary.Id.Value, _testBed.Partner.Id.Value);
        }
    }

    public class TheAddMemberMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenLocalPhone_WhenMemberAdded_ThenRecordsItInE164()
        {
            var sut = _testBed.RegisteredHousehold();

            var added = sut.AddMember(HouseholdMemberId.Generate(), PersonName.Create("Carol Johnson"), EmailAddress.Create("carol@example.com"), new PhoneNumber("(202) 555-0102"), _testBed.StaffUser);

            added.Phone.Should().Be("+12025550102");
        }

        [Fact]
        public void GivenRegisteredHousehold_WhenMemberAdded_ThenMemberIsNonPrimaryAndCountsTowardsSize()
        {
            var sut = _testBed.RegisteredHousehold();

            var memberId = HouseholdMemberId.Generate();
            sut = sut.Decide(a => a.AddMember(memberId, PersonName.Create("Carol Johnson"), EmailAddress.Create("carol@example.com"), new PhoneNumber("(202) 555-0102"), _testBed.StaffUser)).Aggregate;

            sut.Members.Should().ContainSingle(m => m.Id == memberId)
                .Which.IsPrimary.Should().BeFalse();
            sut.Declare().HouseholdSize.Should().Be(3);
        }
    }

    public class TheUpdateMemberMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenUnknownMember_WhenUpdated_ThenThrowsMemberNotFound()
        {
            var sut = _testBed.RegisteredHousehold();

            var act = () => sut.UpdateMember(HouseholdMemberId.Generate(), PersonName.Create("Name"), EmailAddress.Create("e@example.com"), new PhoneNumber("(202) 555-0103"), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.household.members.member_not_found");
        }

        [Fact]
        public void GivenExistingMember_WhenUpdated_ThenReplacesContactDetails()
        {
            var sut = _testBed.RegisteredHousehold();

            sut = sut.Decide(a => a.UpdateMember(_testBed.Partner.Id, PersonName.Create(" Robert Johnson "), EmailAddress.Create("robert@example.com "), new PhoneNumber("(202) 555-0199"), _testBed.StaffUser)).Aggregate;

            var partner = sut.Members.Single(m => m.Id == _testBed.Partner.Id);
            (partner.Name.Value, partner.Email.Value, partner.Phone.Value)
                .Should().Be(("Robert Johnson", "robert@example.com", "+12025550199"));
        }

        [Fact]
        public void GivenUnchangedDetails_WhenUpdated_ThenEmitsNothing()
        {
            var sut = _testBed.RegisteredHousehold();

            (sut, var events) = sut.Decide(a => a.UpdateMember(_testBed.Partner.Id, PersonName.Create("Bob Johnson"), EmailAddress.Create("bob@example.com"), new PhoneNumber("+12025550101"), _testBed.StaffUser));

            events.OfType<HouseholdMemberUpdated>().Should().BeEmpty();
        }
    }

    public class TheRemoveMemberMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenUnknownMember_WhenRemoved_ThenThrowsMemberNotFound()
        {
            var sut = _testBed.RegisteredHousehold();

            var act = () => sut.RemoveMember(HouseholdMemberId.Generate(), _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.household.members.member_not_found");
        }

        [Fact]
        public void GivenPrimaryMember_WhenRemoved_ThenThrowsCannotRemovePrimaryMember()
        {
            var sut = _testBed.RegisteredHousehold();

            var act = () => sut.RemoveMember(_testBed.Primary.Id, _testBed.StaffUser);

            act.Should().Throw<BusinessRuleViolationException>()
                .Which.Code.Should().Be("home_buyers.household.members.cannot_remove_primary_member");
        }

        [Fact]
        public void GivenNonPrimaryMember_WhenRemoved_ThenMemberIsGone()
        {
            var sut = _testBed.RegisteredHousehold();

            sut = sut.Decide(a => a.RemoveMember(_testBed.Partner.Id, _testBed.StaffUser)).Aggregate;

            sut.Members.Should().ContainSingle().Which.Id.Should().Be(_testBed.Primary.Id);
        }
    }

    public class TheUpdateCircumstancesMethod
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNewCircumstances_WhenUpdated_ThenDeclarationReflectsThem()
        {
            var sut = _testBed.RegisteredHousehold();
            var updated = HouseholdCircumstances.Create(true, 72000m);

            sut = sut.Decide(a => a.UpdateCircumstances(updated, _testBed.StaffUser)).Aggregate;

            sut.Declare().Circumstances.Should().Be(updated);
        }

        [Fact]
        public void GivenSameCircumstances_WhenUpdated_ThenEmitsNothing()
        {
            var sut = _testBed.RegisteredHousehold();

            (sut, var events) = sut.Decide(a => a.UpdateCircumstances(HouseholdCircumstances.Create(false, 65000m), _testBed.StaffUser));

            events.OfType<HouseholdCircumstancesUpdated>().Should().BeEmpty();
        }
    }

    public class TheOutstandingReadinessItemsProperty
    {
        private readonly TestBed _testBed = new();

        [Fact]
        public void GivenNothingRecorded_WhenRead_ThenListsEveryItem()
        {
            var sut = _testBed.RegisteredHousehold();

            sut.OutstandingReadinessItems.Should().Equal("HousingCounseling", "CltOrientation", "MortgagePrequalification");
        }

        [Fact]
        public void GivenPrequalificationExpired_WhenRead_ThenListsMortgagePrequalification()
        {
            var expiry = new DateOnly(2026, 12, 31);
            var sut = _testBed.ReadyHousehold(expiry);

            sut = sut.Decide(a => a.ExpirePrequalification(expiry)).Aggregate;

            sut.OutstandingReadinessItems.Should().Equal("MortgagePrequalification");
        }

        [Fact]
        public void GivenEverythingRecorded_WhenRead_ThenIsEmpty()
        {
            var sut = _testBed.ReadyHousehold(new DateOnly(2026, 12, 31));

            sut.OutstandingReadinessItems.Should().BeEmpty();
        }
    }

    public class TheIsPrequalificationValidOnMethod
    {
        private readonly TestBed _testBed = new();

        [Theory]
        [InlineData("2026-12-30", true)]
        [InlineData("2026-12-31", true)]
        [InlineData("2027-01-01", false)]
        public void GivenPrequalificationExpiringEndOfYear_WhenCheckedOnDate_ThenValidThroughExpiryDate(string date, bool expected)
        {
            var sut = _testBed.ReadyHousehold(new DateOnly(2026, 12, 31));

            sut.IsPrequalificationValidOn(DateOnly.Parse(date, System.Globalization.CultureInfo.InvariantCulture))
                .Should().Be(expected);
        }
    }
}
