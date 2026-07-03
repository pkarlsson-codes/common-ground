using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Command that archives a ground lease template.</summary>
/// <param name="TemplateId">The ID of the template to archive.</param>
/// <param name="ArchivedBy">The ID of the user performing the action.</param>
public sealed record ArchiveGroundLeaseTemplate(GroundLeaseTemplateId TemplateId, UserId ArchivedBy);

/// <summary>Handles <see cref="ArchiveGroundLeaseTemplate"/> commands.</summary>
public static class ArchiveGroundLeaseTemplateHandler
{
    /// <summary>Archives the ground lease template identified by the command.</summary>
    /// <param name="command">The archive command.</param>
    /// <param name="session">The Marten document session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public static async Task Handle(
        ArchiveGroundLeaseTemplate command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var template = await session
            .LoadAggregateAsync<GroundLeaseTemplate>(command.TemplateId, cancellationToken)
            .ConfigureAwait(false);

        template.Archive(command.ArchivedBy);

        session.UpdateAggregate(template);
    }
}
