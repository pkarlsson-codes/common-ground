using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Persistence;

namespace CommonGround.PropertyRegistry.Application.Commands;

/// <summary>Initiates a new development project for a land parcel.</summary>
/// <param name="LandParcelId">The parcel to associate the project with.</param>
/// <param name="Name">The name of the development project.</param>
/// <param name="Description">A description of the development project.</param>
/// <param name="InitiatedBy">The staff user initiating the project.</param>
public sealed record InitiateNewConstructionProject(
    LandParcelId LandParcelId,
    string Name,
    string? Description,
    UserId InitiatedBy);

/// <summary>Handles <see cref="InitiateNewConstructionProject"/> by creating and persisting a new DevelopmentProject.</summary>
public static class InitiateNewConstructionProjectHandler
{
    /// <summary>Creates a new DevelopmentProject, persists it, and returns its ID.</summary>
    public static async Task<DevelopmentProjectId> Handle(
        InitiateNewConstructionProject command,
        IDevelopmentProjectRepository repository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var project = DevelopmentProject.InitiateNewConstruction(
            command.LandParcelId,
            command.Name,
            command.Description,
            command.InitiatedBy);

        repository.Stage(project);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return project.Id;
    }
}
