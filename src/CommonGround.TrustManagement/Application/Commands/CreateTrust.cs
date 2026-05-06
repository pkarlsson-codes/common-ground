using CommonGround.TrustManagement.Domain.Trusts;
using Marten;

namespace CommonGround.TrustManagement.Application.Commands;

/// <summary>Creates a new trust with the specified name.</summary>
/// <param name="Name">The name of the trust to create.</param>
public sealed record CreateTrust(string Name);

/// <summary>Result of <see cref="CreateTrust"/>.</summary>
public sealed record CreateTrustResult(string TrustId);

/// <summary>Handles <see cref="CreateTrust"/>.</summary>
public static class CreateTrustHandler
{
    /// <summary>Creates a new trust and returns its stable ID.</summary>
    public static Task<CreateTrustResult> Handle(
        CreateTrust command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var trust = Trust.Create(command.Name);
        trust.ValidateInvariants();
        session.Events.StartStream<Trust>(trust.Id.Value, trust.UncommittedEvents.ToArray());
        trust.UncommittedEvents.MarkAsCommitted();

        return Task.FromResult(new CreateTrustResult(trust.Id.Value));
    }
}
