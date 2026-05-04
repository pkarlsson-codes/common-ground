using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.TrustManagement.Domain.Trusts.Events;

namespace CommonGround.TrustManagement.Domain.Trusts;

/// <summary>Represents a trust.</summary>
public sealed class Trust : AggregateRoot<TrustId>
{
    /// <summary>The name of the trust.</summary>
    public string Name { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Trust() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>Creates a new trust.</summary>
    public static Trust Create(string name)
    {
        var trust = new Trust();

        trust.ApplyChange(new TrustCreated(TrustId.Generate().Value, name));
        return trust;
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case TrustCreated e: Apply(e); break;
            default:
                throw new InvalidOperationException($"Unhandled event type: {domainEvent.GetType().Name}");
        }
    }

    private void Apply(TrustCreated e)
    {
        Id = TrustId.Create(e.AggregateId);
        Name = e.Name;
    }
}
