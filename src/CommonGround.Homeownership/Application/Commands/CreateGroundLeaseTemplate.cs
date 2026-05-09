using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.SharedKernel.Domain;
using Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Creates a new ground lease template.</summary>
public sealed record CreateGroundLeaseTemplate(
    string TemplateName,
    int TermYears,
    string CovenantTerms,
    ResalePolicyTerms ResalePolicyTerms,
    UserId CreatedBy);

/// <summary>Handles <see cref="CreateGroundLeaseTemplate"/>.</summary>
public static class CreateGroundLeaseTemplateHandler
{
    /// <summary>Creates a new ground lease template and returns its ID.</summary>
    public static Task<GroundLeaseTemplateId> Handle(
        CreateGroundLeaseTemplate command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var template = GroundLeaseTemplate.Create(
            command.TemplateName,
            command.TermYears,
            command.CovenantTerms,
            command.ResalePolicyTerms,
            command.CreatedBy);

        session.Events.StartStream<GroundLeaseTemplate>(template.Id.Value, template.UncommittedEvents.ToArray());
        template.UncommittedEvents.MarkAsCommitted();

        return Task.FromResult(template.Id);
    }
}
