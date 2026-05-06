using CommonGround.PropertyRegistry.Domain.DevelopmentProjects;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using Marten;

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
    public static Task<DevelopmentProjectId> Handle(
        InitiateNewConstructionProject command,
        IDocumentSession session,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var project = DevelopmentProject.InitiateNewConstruction(
            command.LandParcelId,
            command.Name,
            command.Description,
            command.InitiatedBy);

        project.ValidateInvariants();
        session.Events.StartStream<DevelopmentProject>(project.Id.Value, project.UncommittedEvents.ToArray());
        project.UncommittedEvents.MarkAsCommitted();

        return Task.FromResult(project.Id);
    }
}
