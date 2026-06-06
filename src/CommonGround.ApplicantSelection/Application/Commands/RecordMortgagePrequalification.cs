using CommonGround.ApplicantSelection.Domain.HomeownerApplications;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.ApplicantSelection.Application.Commands;

/// <summary>Records a mortgage prequalification for a homeowner application.</summary>
public sealed record RecordMortgagePrequalification(
    HomeownerApplicationId ApplicationId,
    string LenderName,
    decimal ApprovedAmount,
    DateOnly ExpiryDate,
    UserId RecordedBy);

/// <summary>Handles <see cref="RecordMortgagePrequalification"/>.</summary>
public static class RecordMortgagePrequalificationHandler
{
    /// <summary>Loads the application, records the prequalification, and persists it.</summary>
    public static async Task Handle(
        RecordMortgagePrequalification command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var application = await session
            .LoadAggregateAsync<HomeownerApplication>(command.ApplicationId, cancellationToken)
            .ConfigureAwait(false);

        application.RecordMortgagePrequalification(
            command.LenderName,
            command.ApprovedAmount,
            command.ExpiryDate,
            command.RecordedBy);

        session.UpdateAggregate(application);
    }
}
