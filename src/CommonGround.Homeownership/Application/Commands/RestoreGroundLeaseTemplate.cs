using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Command that restores an archived ground lease template.</summary>
/// <param name="TemplateId">The ID of the template to restore.</param>
/// <param name="RestoredBy">The ID of the user performing the action.</param>
public sealed record RestoreGroundLeaseTemplate(GroundLeaseTemplateId TemplateId, UserId RestoredBy);

/// <summary>Handles <see cref="RestoreGroundLeaseTemplate"/> commands.</summary>
public static class RestoreGroundLeaseTemplateHandler
{
    /// <summary>Restores the ground lease template identified by the command.</summary>
    /// <param name="command">The restore command.</param>
    /// <param name="session">The Marten document session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public static async Task Handle(
        RestoreGroundLeaseTemplate command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var template = await session
            .LoadAggregateAsync<GroundLeaseTemplate>(command.TemplateId, cancellationToken)
            .ConfigureAwait(false);

        template.Restore(command.RestoredBy);

        session.UpdateAggregate(template);
    }
}
