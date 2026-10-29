using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.HomeSelections.Events;

/// <summary>
/// A household was drawn by lottery from <paramref name="PoolApplicationIds"/>, either the whole
/// candidate list or only the candidates staff found tied. Drawing again with
/// <paramref name="Seed"/> over the same pool gives the same winner.
/// </summary>
public sealed record HouseholdSelectedByLottery(
    string AggregateId,
    string ApplicationId,
    IReadOnlyList<string> PoolApplicationIds,
    int Seed,
    string DrawnBy) : DomainEvent(AggregateId);
