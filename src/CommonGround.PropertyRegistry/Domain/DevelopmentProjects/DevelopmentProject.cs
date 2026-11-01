using CommonGround.PropertyRegistry.Domain.DevelopmentProjects.Events;
using CommonGround.PropertyRegistry.Application.Errors;
using CommonGround.PropertyRegistry.Domain.Homes;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects;

/// <summary>
/// A development project associated with a CLT land parcel.
/// Tracks the intent and status of development activity on a parcel.
/// </summary>
public sealed class DevelopmentProject : AggregateRoot<DevelopmentProjectId>
{
    private readonly List<HomeId> _homeIds = [];

#pragma warning disable CS8618 // State is set by Create from the stream's first event.
    private DevelopmentProject() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the land parcel this project is for.</summary>
    public LandParcelId LandParcelId { get; private set; }

    /// <summary>Gets the name of the development project.</summary>
    public string Name { get; private set; }

    /// <summary>Gets the description of the development project.</summary>
    public string? Description { get; private set; }

    /// <summary>Gets the IDs of homes associated with this development project.</summary>
    public IReadOnlyCollection<HomeId> HomeIds => _homeIds.AsReadOnly();

    /// <summary>Gets the type of development project.</summary>
    public ProjectType ProjectType { get; private set; }

    /// <summary>Gets the current status of this project.</summary>
    public ProjectStatus ProjectStatus { get; private set; }

    /// <summary>Initiates a new development project for a land parcel.</summary>
    /// <param name="id">The new stream's ID, chosen by the caller.</param>
    /// <param name="landParcelId">The parcel to associate the project with.</param>
    /// <param name="name">The name of the development project.</param>
    /// <param name="description">A description of the development project.</param>
    /// <param name="initiatedBy">The staff user initiating the project.</param>
    public static DevelopmentProjectInitiated InitiateNewConstruction(
        DevelopmentProjectId id,
        LandParcelId landParcelId,
        string name,
        string? description,
        UserId initiatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw BusinessRuleViolation.DevelopmentProject.Initiate.NameRequired();

        if (string.IsNullOrWhiteSpace(description))
            throw BusinessRuleViolation.DevelopmentProject.Initiate.DescriptionRequired();

        return new DevelopmentProjectInitiated(
            id.Value,
            landParcelId.Value,
            [],
            name,
            description,
            ProjectType.NewConstruction.Value,
            initiatedBy.Value);
    }

    /// <summary>Initiates a new rehabilitation development project for a land parcel.</summary>
    /// <param name="id">The new stream's ID, chosen by the caller.</param>
    /// <param name="landParcelId">The parcel to associate the project with.</param>    
    /// <param name="name">The name of the development project.</param>
    /// <param name="description">A description of the development project.</param>
    /// <param name="homeIds">The homes associated with the rehabilitation project.</param>
    /// <param name="initiatedBy">The staff user initiating the project.</param>
    public static DevelopmentProjectInitiated InitiateRehabilitation(
        DevelopmentProjectId id,
        LandParcelId landParcelId,
        string name,
        string? description,
        IReadOnlyCollection<HomeId> homeIds,
        UserId initiatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw BusinessRuleViolation.DevelopmentProject.Initiate.NameRequired();

        if (string.IsNullOrWhiteSpace(description))
            throw BusinessRuleViolation.DevelopmentProject.Initiate.DescriptionRequired();

        if (homeIds == null || homeIds.Count == 0)
            throw BusinessRuleViolation.DevelopmentProject.InitiateRehabilitation.HomeIdsRequired();

        return new DevelopmentProjectInitiated(
            id.Value,
            landParcelId.Value,
            [.. homeIds.Select(h => h.Value)],
            name,
            description,
            ProjectType.Rehabilitation.Value,
            initiatedBy.Value);
    }

    /// <summary>Starts a development project from its first event.</summary>
    public static DevelopmentProject Create(DevelopmentProjectInitiated e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var developmentProject = new DevelopmentProject();
        developmentProject.Apply(e);
        return developmentProject;
    }

    private void Apply(DevelopmentProjectInitiated e)
    {
        Id = DevelopmentProjectId.Create(e.AggregateId);
        LandParcelId = LandParcelId.Create(e.LandParcelId);
        _homeIds.AddRange(e.HomeIds.Select(HomeId.Create));
        Name = e.Name;
        Description = e.Description;
        ProjectStatus = ProjectStatus.Initiated;
        ProjectType = ProjectType.FromValue(e.ProjectType);
    }
}
