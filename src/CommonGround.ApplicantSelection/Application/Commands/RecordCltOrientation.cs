using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Records CLT orientation completion for a homeowner application.</summary>
public sealed record RecordCltOrientation(
    HomeownerApplicationId ApplicationId,
    DateOnly CompletedDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordCltOrientation"/>.</summary>
public static class RecordCltOrientationHandler
{
    /// <summary>Loads the application, records orientation completion, and persists it.</summary>
    public static async Task Handle(
        RecordCltOrientation command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.RecordCltOrientation(command.CompletedDate, command.RecordedBy);

        session.UpdateAggregate(application);
    }
}