using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeSelections.Events;
using CommonGround.HomeBuyers.Domain.Households;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>
/// Staff choosing which household on the waiting list gets a home, either by citing
/// <see cref="SelectionPriority"/> values or by lottery. Staff can cancel it without choosing
/// anyone, after which a new selection can start for the same home.
/// </summary>
public sealed class HomeSelection : AggregateRoot<HomeSelectionId>
{
#pragma warning disable CS8618
    private HomeSelection() { }
#pragma warning restore CS8618

    public string HomeId { get; private set; }
    public HomeSelectionStatus Status { get; private set; }
    public UserId StartedBy { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public IReadOnlyList<SelectionCandidate> Candidates { get; private set; }
    public HomeownerApplicationId? SelectedApplicationId { get; private set; }
    public string? CancellationReason { get; private set; }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case HomeSelectionStarted e: Apply(e); break;
            case HouseholdSelectedByPriority e: Apply(e); break;
            case HouseholdSelectedByLottery e: Apply(e); break;
            case HomeSelectionCancelled e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(HomeSelectionStarted e)
    {
        Id = HomeSelectionId.Create(e.AggregateId);
        HomeId = e.HomeId;
        Status = HomeSelectionStatus.Open;
        StartedBy = UserId.Create(e.StartedBy);
        StartedAt = e.OccurredAt;
        Candidates = e.Candidates
            .Select(c => SelectionCandidate.Create(
                HomeownerApplicationId.Create(c.ApplicationId),
                HouseholdId.Create(c.HouseholdId),
                c.AnnualIncome,
                c.HouseholdSize,
                c.RequiresAccessibility,
                c.WaitingSince))
            .ToList();
    }

    private void Apply(HouseholdSelectedByPriority e)
    {
        SelectedApplicationId = HomeownerApplicationId.Create(e.ApplicationId);
        Status = HomeSelectionStatus.Completed;
    }

    private void Apply(HouseholdSelectedByLottery e)
    {
        SelectedApplicationId = HomeownerApplicationId.Create(e.ApplicationId);
        Status = HomeSelectionStatus.Completed;
    }

    private void Apply(HomeSelectionCancelled e)
    {
        CancellationReason = e.Reason;
        Status = HomeSelectionStatus.Cancelled;
    }
}
