using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using Wolverine.Marten;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Initiates a new development project for a land parcel.</summary>
/// <param name="DevelopmentProjectId">The new project's ID, chosen by the caller.</param>
/// <param name="LandParcelId">The parcel to associate the project with.</param>
/// <param name="Name">The name of the development project.</param>
/// <param name="Description">A description of the development project.</param>
/// <param name="InitiatedBy">The staff user initiating the project.</param>
public sealed record InitiateNewConstructionProject(
    DevelopmentProjectId DevelopmentProjectId,
    LandParcelId LandParcelId,
    string Name,
    string? Description,
    UserId InitiatedBy);

/// <summary>Handles <see cref="InitiateNewConstructionProject"/> by creating and persisting a new DevelopmentProject.</summary>
public static class InitiateNewConstructionProjectHandler
{
    /// <summary>Starts the new project's stream.</summary>
    public static IStartStream Handle(InitiateNewConstructionProject command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var initiated = DevelopmentProject.InitiateNewConstruction(
            command.DevelopmentProjectId,
            command.LandParcelId,
            command.Name,
            command.Description,
            command.InitiatedBy);

        return MartenOps.StartStream<DevelopmentProject>(command.DevelopmentProjectId.Value, initiated);
    }
}
