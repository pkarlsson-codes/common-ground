using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>
/// Command to initiate a new rehabilitation project for a land parcel and associated homes.
/// </summary>
/// <param name="DevelopmentProjectId">The new project's ID, chosen by the caller.</param>
/// <param name="LandParcelId">The parcel to associate the project with.</param>
/// <param name="HomeIds">The homes to include in the rehabilitation project.</param>
/// <param name="Name">The name of the rehabilitation project.</param>
/// <param name="Description">A description of the rehabilitation project.</param>
/// <param name="InitiatedBy">The staff user initiating the project.</param>
public sealed record InitiateRehabilitationProject(
    DevelopmentProjectId DevelopmentProjectId,
    LandParcelId LandParcelId,
    IReadOnlyList<HomeId> HomeIds,
    string Name,
    string? Description,
    UserId InitiatedBy);

/// <summary>Handles <see cref="InitiateRehabilitationProject"/>.</summary>
public static class InitiateRehabilitationProjectHandler
{
    /// <summary>Starts the new project's stream.</summary>
    public static IStartStream Handle(InitiateRehabilitationProject command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var initiated = DevelopmentProject.InitiateRehabilitation(
            command.DevelopmentProjectId,
            command.LandParcelId,
            command.Name,
            command.Description,
            command.HomeIds,
            command.InitiatedBy);

        return MartenOps.StartStream<DevelopmentProject>(command.DevelopmentProjectId.Value, initiated);
    }
}
