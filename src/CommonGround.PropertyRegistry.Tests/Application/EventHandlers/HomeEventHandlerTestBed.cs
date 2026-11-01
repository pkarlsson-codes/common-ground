using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.Homes.Events;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.Testing;
using JasperFx.Events;

namespace CommonGround.PropertyRegistry.Tests.Application.EventHandlers;

/// <summary>Homes in each status, as streams, for the handlers that react to other contexts' events.</summary>
public class HomeEventHandlerTestBed
{
    public HomeId HomeId { get; } = HomeId.Generate();

    public DateOnly Today { get; } = new(2026, 10, 1);

    public StubEventStream<Home> Available() => new(AvailableHome());

    public StubEventStream<Home> InSelection() => new(AvailableHome().Evolve(new HomeSelectionOpened(HomeId.Value, "selection-1", Today))!);

    public StubEventStream<Home> UnderContract() =>
        new(AvailableHome().Evolve(
            new HomeSelectionOpened(HomeId.Value, "selection-1", Today),
            new HomeUnderContract(HomeId.Value, Today.AddDays(7)))!);

    public StubEventStream<Home> Leased() =>
        new(AvailableHome().Evolve(new HomeLeased(HomeId.Value, Today.AddDays(60)))!);

    private Home AvailableHome()
    {
        var staff = UserId.Generate();
        var assessment = UnitAssessment.Create(280000m, new DateOnly(2024, 1, 1), null, null, staff, DateTimeOffset.UtcNow, Today);
        return EventReplay.Replay<Home>(
            Home.Register(HomeId, LandParcelId.Generate(), null, HomeType.SingleFamily, 3, 2m, true, 1200m, 1985, assessment, staff));
    }
}
