using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using DddNamespace.DddPluralFolder.Events;

namespace DddNamespace.DddPluralFolder;

/// <summary>TODO: Describe the DddAggregate aggregate.</summary>
public sealed class DddAggregate : AggregateRoot<DddAggregateId>
{
#pragma warning disable CS8618
    private DddAggregate() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the user who created this DddAggregate.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>Creates a new DddAggregate.</summary>
    public static DddAggregate Create(UserId createdBy)
    {
        ArgumentNullException.ThrowIfNull(createdBy);

        var aggregate = new DddAggregate();

        aggregate.ApplyChange(new DddAggregateCreated(
            DddAggregateId.Generate().Value,
            createdBy.Value));

        return aggregate;
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case DddAggregateCreated e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(DddAggregateCreated e)
    {
        Id = DddAggregateId.Create(e.AggregateId);
        CreatedBy = UserId.Create(e.CreatedBy);
    }
}
