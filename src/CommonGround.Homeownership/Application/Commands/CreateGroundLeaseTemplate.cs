using CommonGround.Homeownership.Domain.GroundLeases.LeaseFees;
using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.Homeownership.Domain.GroundLeases.ResaleFormulas;
using CommonGround.Homeownership.Domain.GroundLeaseTemplates;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.Homeownership.Application.Commands;

/// <summary>Creates a new ground lease template.</summary>
public sealed record CreateGroundLeaseTemplate(
    GroundLeaseTemplateId TemplateId,
    string TemplateName,
    int TermYears,
    int OptionWindowDays,
    string CovenantTerms,
    ResalePolicyTerms ResalePolicyTerms,
    LeaseFeeTerms LeaseFeeTerms,
    MortgageDebtCap MortgageDebtCap,
    UserId CreatedBy);

/// <summary>Handles <see cref="CreateGroundLeaseTemplate"/>.</summary>
public static class CreateGroundLeaseTemplateHandler
{
    /// <summary>Starts the new stream.</summary>
    public static IStartStream Handle(CreateGroundLeaseTemplate command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var started = GroundLeaseTemplate.Define(
            command.TemplateId,
            command.TemplateName,
            command.TermYears,
            command.OptionWindowDays,
            command.CovenantTerms,
            command.ResalePolicyTerms,
            command.LeaseFeeTerms,
            command.MortgageDebtCap,
            command.CreatedBy);

        return MartenOps.StartStream<GroundLeaseTemplate>(command.TemplateId.Value, started);
    }
}
