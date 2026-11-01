using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;
using CommonGround.SharedKernel.Domain;
using Marten;
using Wolverine.Marten;

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
    /// <param name="template">The template, loaded from its stream.</param>
    public static GroundLeaseTemplateRestored Handle(RestoreGroundLeaseTemplate command, [WriteAggregate(nameof(RestoreGroundLeaseTemplate.TemplateId))] GroundLeaseTemplate template)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(template);

        return template.Restore(command.RestoredBy);
    }
}
