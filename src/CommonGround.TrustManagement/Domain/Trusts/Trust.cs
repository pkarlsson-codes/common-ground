using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.TrustManagement.Application.Errors;
using CommonGround.TrustManagement.Domain.Trusts.Events;

namespace CommonGround.TrustManagement.Domain.Trusts;

/// <summary>Represents a trust.</summary>
public sealed class Trust : AggregateRoot<TrustId>
{
    /// <summary>The name of the trust.</summary>
    public string Name { get; private set; }

    /// <summary>The time zone the trust operates in; "today" and dates derived from timestamps follow it.</summary>
    public IanaTimeZone TimeZone { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Trust() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>Creates a new trust.</summary>
    public static Trust Create(string name, IanaTimeZone timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw BusinessRuleViolation.Trust.Create.NameRequired();
        }

        var trust = new Trust();

        trust.ApplyChange(new TrustCreated(TrustId.Generate().Value, name, timeZone.Value));
        return trust;
    }

    /// <summary>Raises an onboarding event that links this trust to an authenticated creator user.</summary>
    public void RequestOnboarding(UserId creatorUserId, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(creatorUserId);

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw BusinessRuleViolation.Trust.RequestOnboarding.CorrelationIdRequired();
        }

        ApplyChange(new TrustOnboardingRequested(Id.Value, Name, creatorUserId.Value, correlationId));
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case TrustCreated e: Apply(e); break;
            case TrustOnboardingRequested e: Apply(e); break;
            default:
                throw new InvalidOperationException($"Unhandled event type: {domainEvent.GetType().Name}");
        }
    }

    private void Apply(TrustCreated e)
    {
        Id = TrustId.Create(e.AggregateId);
        Name = e.Name;
        TimeZone = IanaTimeZone.Create(e.TimeZone);
    }

    private static void Apply(TrustOnboardingRequested _)
    {
        // No additional state transition on Trust; this event exists for integration choreography.
    }
}
