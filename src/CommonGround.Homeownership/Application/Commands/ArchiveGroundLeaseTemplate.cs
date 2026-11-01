using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;
using CommonGround.SharedKernel.Domain;
using Marten;
using Wolverine.Marten;

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
    /// <param name="template">The template, loaded from its stream.</param>
    public static GroundLeaseTemplateArchived Handle(ArchiveGroundLeaseTemplate command, [WriteAggregate(nameof(ArchiveGroundLeaseTemplate.TemplateId))] GroundLeaseTemplate template)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(template);

        return template.Archive(command.ArchivedBy);
    }
}
