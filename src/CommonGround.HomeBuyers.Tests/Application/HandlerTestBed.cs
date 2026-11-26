using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.Testing;

namespace CommonGround.HomeBuyers.Tests.Application;

/// <summary>Households and applications as streams, for the handlers that write to several at once.</summary>
public class HandlerTestBed
{
    public UserId StaffUser { get; } = UserId.Generate();

    public DateOnly SelectedDate { get; } = new(2026, 7, 20);

    public const string HomeId = "home-1";

    public Household ReadyHousehold() => NewHousehold(ready: true);

    public Household NewHousehold(bool ready)
    {
        var household = EventReplay.Replay<Household>(global::CommonGround.HomeBuyers.Domain.Households.Household.Register(
            HouseholdId.Generate(),
            [HouseholdMember.Create(PersonName.Create("Alice Johnson"), EmailAddress.Create("alice@example.com"), new PhoneNumber("(202) 555-0100"), true)],
            HouseholdCircumstances.Create(false, Money.Create(65000m)),
            StaffUser));
        return ready
            ? household.Evolve(
                household.RecordHousingCounseling("CERT-001", new DateOnly(2026, 6, 2), "Counseling Org", new DateOnly(2026, 6, 10), StaffUser)!,
                household.RecordCltOrientation(new DateOnly(2026, 6, 3), new DateOnly(2026, 6, 10), StaffUser)!,
                household.RecordMortgagePrequalification("Lender", 210000m, new DateOnly(2027, 1, 1), StaffUser))!
            : household;
    }

    public HomeownerApplication ApprovedApplication(Household household)
    {
        var submitted = HomeownerApplication.Submit(HomeownerApplicationId.Generate(), household.Id, household.Declare(), null, StaffUser);
        var application = EventReplay.Replay<HomeownerApplication>(submitted);
        return application.Evolve(application.Approve(new DateOnly(2026, 6, 1), null, StaffUser))!;
    }

    public HomeownerApplication SelectedApplication(Household household)
    {
        var application = ApprovedApplication(household);
        return application.Evolve(application.SelectForHome(
            household, HomeSelectionId.Generate(), HomeId, "land-parcel-1", "template-1", 200000m, SelectedDate, StaffUser,
            incomeLimitPercentOfAmi: null, incomeLimits: null))!;
    }
}
