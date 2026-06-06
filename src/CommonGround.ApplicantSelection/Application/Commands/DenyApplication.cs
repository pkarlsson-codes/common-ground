using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Denies a homeowner application.</summary>
public sealed record DenyApplication(
    HomeownerApplicationId ApplicationId,
    string Reason,
    DateOnly DeniedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="DenyApplication"/>.</summary>
public static class DenyApplicationHandler
{
    /// <summary>Loads the application, denies it, and persists it.</summary>
    public static async Task Handle(
        DenyApplication command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.DenyApplication(command.Reason, command.DeniedDate, command.RecordedBy);

        session.UpdateAggregate(application);
    }
}