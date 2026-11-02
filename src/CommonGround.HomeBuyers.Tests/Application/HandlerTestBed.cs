using CommonGround.HomeBuyers.Application.Commands;
using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.Testing;
using JasperFx.Events;

namespace CommonGround.HomeBuyers.Tests.Application;

/// <summary>Households, applications and selections as streams, for the handlers that write to several at once.</summary>
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
            [HouseholdMember.Create("Alice Johnson", "alice@example.com", "555-0100", true)],
            HouseholdCircumstances.Create(false, 65000m),
            StaffUser));
        return ready
            ? household.Evolve(
                household.RecordHousingCounseling("CERT-001", new DateOnly(2026, 6, 2), "Counseling Org", StaffUser)!,
                household.RecordCltOrientation(new DateOnly(2026, 6, 3), StaffUser)!,
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
        return application.Evolve(application.SelectForHome(household, HomeSelectionId.Generate(), HomeId, "land-parcel-1", "template-1", 200000m, SelectedDate, StaffUser))!;
    }

    public HomeSelection OpenSelection(params (HomeownerApplication Application, Household Household)[] candidates) =>
        EventReplay.Replay<HomeSelection>(HomeSelection.Start(
            HomeSelectionId.Generate(),
            HomeId,
            [.. candidates.Select(c => SelectionCandidate.Create(
                c.Application.Id, c.Household.Id, 65000m, 1, false, new DateOnly(2026, 6, 1)))],
            null,
            StaffUser));

    public SelectionCompletion Completion(HomeSelection selection, params (HomeownerApplication Application, Household Household)[] candidates) =>
        new(
            new StubEventStream<HomeSelection>(selection),
            [.. candidates.Select(c => new CandidateApplication(new StubEventStream<HomeownerApplication>(c.Application), c.Household))],
            new HomeFactsView { Id = HomeId, LandParcelId = "land-parcel-1" });
}
