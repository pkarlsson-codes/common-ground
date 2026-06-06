using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Expires a mortgage prequalification if it is still current.</summary>
public sealed record ExpireMortgagePrequalification(
    HomeownerApplicationId ApplicationId,
    DateOnly OriginalExpiryDate);

/// <summary>Handles <see cref="ExpireMortgagePrequalification"/>.</summary>
public static class ExpireMortgagePrequalificationHandler
{
    /// <summary>Loads the application, expires the prequalification when applicable, and persists it.</summary>
    public static async Task Handle(
        ExpireMortgagePrequalification command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        var changed = application.ExpirePrequalification(command.OriginalExpiryDate);
        if (changed)
            session.UpdateAggregate(application);
    }
}
