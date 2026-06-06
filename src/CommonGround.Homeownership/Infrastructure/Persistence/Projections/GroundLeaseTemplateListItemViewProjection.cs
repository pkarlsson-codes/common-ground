using CommonGround.Homeownership.Application.ReadModels;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Homeownership.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="GroundLeaseTemplateListItemView"/> from GroundLeaseTemplate events.</summary>
internal sealed class GroundLeaseTemplateListItemViewProjection : SingleStreamProjection<GroundLeaseTemplateListItemView, string>
{
    /// <summary>
    /// Creates a new <see cref="GroundLeaseTemplateListItemView"/> when a <see cref="GroundLeaseTemplateCreated"/> event is applied, initializing the view with the template's ID, name, and setting the status to "Active".
    /// </summary>
    /// <param name="e">Ground lease template created event..</param>
    /// <returns>A new instance of <see cref="GroundLeaseTemplateListItemView"/> initialized with the event data.</returns>
    public static GroundLeaseTemplateListItemView Create(GroundLeaseTemplateCreated e) =>
        new()
        {
            Id = e.AggregateId,
            TemplateName = e.TemplateName,
            Status = "Active",
        };

    /// <summary>
    /// Marks the template as inactive in the list view when it is archived, as the list view only shows active templates.
    /// </summary>
    /// <param name="view">The list item view to update.</param>
    /// <param name="e">Ground lease template archived event.</param>
    public static void Apply(GroundLeaseTemplateListItemView view, GroundLeaseTemplateArchived e)
    {
        view.Status = "Inactive";
    }

    /// <summary>
    /// Reactivates the template in the list view when a template is restored, as the list view only shows active templates.
    /// </summary>
    /// <param name="view">The list item view to update.</param>
    /// <param name="e">Ground lease template restored event.</param>
    public static void Apply(GroundLeaseTemplateListItemView view, GroundLeaseTemplateRestored e)
    {
        view.Status = "Active";
    }
}