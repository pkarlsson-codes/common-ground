using CommonGround.SharedKernel.Domain;
using CommonGround.TrustManagement.Domain.Trusts;
using Wolverine.Marten;

namespace CommonGround.TrustManagement.Application.Commands;

/// <summary>Creates a new trust with the specified name.</summary>
/// <param name="TrustId">The new trust's ID, chosen by the caller.</param>
/// <param name="Name">The name of the trust to create.</param>
/// <param name="TimeZone">The IANA time zone id the trust operates in.</param>
/// <param name="CreatorUserId">The authenticated user initiating trust creation.</param>
/// <param name="CorrelationId">Correlation id used for cross-BC tracing and idempotency context.</param>
public sealed record CreateTrust(TrustId TrustId, string Name, string TimeZone, UserId CreatorUserId, string CorrelationId);

/// <summary>Handles <see cref="CreateTrust"/>.</summary>
public static class CreateTrustHandler
{
    /// <summary>Starts the trust's stream with its creation and the creator's onboarding.</summary>
    public static IStartStream Handle(CreateTrust command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var created = Trust.Establish(command.TrustId, command.Name, IanaTimeZone.Create(command.TimeZone));
        var onboarding = Trust.Create(created).RequestOnboarding(command.CreatorUserId, command.CorrelationId);

        return MartenOps.StartStream<Trust>(command.TrustId.Value, created, onboarding);
    }
}
