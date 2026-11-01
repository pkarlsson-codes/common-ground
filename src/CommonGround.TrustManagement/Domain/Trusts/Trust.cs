using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Application.Errors;
using CommonGround.TrustManagement.Domain.Trusts.Events;

namespace CommonGround.TrustManagement.Domain.Trusts;

/// <summary>Represents a trust.</summary>
public sealed class Trust : AggregateRoot<TrustId>
{
#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private Trust() { }
#pragma warning restore CS8618

    /// <summary>The name of the trust.</summary>
    public string Name { get; private set; }

    /// <summary>The time zone the trust operates in; "today" and dates derived from timestamps follow it.</summary>
    public IanaTimeZone TimeZone { get; private set; }

    /// <summary>Decides to establish a new trust.</summary>
    /// <returns>The event that starts the trust's stream.</returns>
    public static TrustCreated Establish(TrustId id, string name, IanaTimeZone timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw BusinessRuleViolation.Trust.Create.NameRequired();
        }

        return new TrustCreated(id.Value, name, timeZone.Value);
    }

    /// <summary>Decides to link this trust to the authenticated user who created it.</summary>
    public TrustOnboardingRequested RequestOnboarding(UserId creatorUserId, string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw BusinessRuleViolation.Trust.RequestOnboarding.CorrelationIdRequired();
        }

        return new TrustOnboardingRequested(Id.Value, Name, creatorUserId.Value, correlationId);
    }

    /// <summary>Starts a trust from its first event.</summary>
    public static Trust Create(TrustCreated e)
    {
        ArgumentNullException.ThrowIfNull(e);

        return new Trust
        {
            Id = TrustId.Create(e.AggregateId),
            Name = e.Name,
            TimeZone = IanaTimeZone.Create(e.TimeZone),
        };
    }

    /// <summary>
    /// Applies <see cref="TrustOnboardingRequested"/>. Nothing changes on the trust; the event exists for
    /// integration choreography.
    /// </summary>
#pragma warning disable CA1822 // Marten dispatches events to instance Apply methods.
    public void Apply(TrustOnboardingRequested e)
    {
    }
#pragma warning restore CA1822
}
