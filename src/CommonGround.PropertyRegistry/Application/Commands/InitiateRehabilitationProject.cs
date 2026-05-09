using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Extensions;
using Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>
/// Command to initiate a new rehabilitation project for a land parcel and associated homes.
/// </summary>
/// <param name="LandParcelId">The parcel to associate the project with.</param>
/// <param name="HomeIds">The homes to include in the rehabilitation project.</param>
/// <param name="Name">The name of the rehabilitation project.</param>
/// <param name="Description">A description of the rehabilitation project.</param>
/// <param name="InitiatedBy">The staff user initiating the project.</param>
public sealed record InitiateRehabilitationProject(
    LandParcelId LandParcelId,
    IReadOnlyList<HomeId> HomeIds,
    string Name,
    string? Description,
    UserId InitiatedBy);

/// <summary>Handles <see cref="InitiateRehabilitationProject"/>.</summary>
public static class InitiateRehabilitationProjectHandler
{
    /// <summary>Creates a new DevelopmentProject of type Rehabilitation, persists it, and returns its ID.</summary>
    public static DevelopmentProjectId Handle(
        InitiateRehabilitationProject command,
        IDocumentSession session)
    {
        ArgumentNullException.ThrowIfNull(command);

        var project = DevelopmentProject.InitiateRehabilitation(
            command.LandParcelId,
            command.Name,
            command.Description,
            command.HomeIds,
            command.InitiatedBy);

        session.CreateAggregate<DevelopmentProject, DevelopmentProjectId>(project);

        return project.Id;
    }
}
