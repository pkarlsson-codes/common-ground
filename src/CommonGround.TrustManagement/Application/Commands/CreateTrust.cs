using CommonGround.SharedKernel.Persistence;
using CommonGround.TrustManagement.Domain.Trusts;

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
    public static async Task<CreateTrustResult> Handle(
        CreateTrust command,
        ITrustRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var trust = Trust.Create(command.Name);

        repository.Stage(trust);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new CreateTrustResult(trust.Id.Value);
    }
}