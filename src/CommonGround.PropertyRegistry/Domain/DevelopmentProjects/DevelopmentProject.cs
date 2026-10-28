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

#pragma warning disable CS8618
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
    /// <param name="landParcelId">The parcel to associate the project with.</param>
    /// <param name="name">The name of the development project.</param>
    /// <param name="description">A description of the development project.</param>
    /// <param name="initiatedBy">The staff user initiating the project.</param>
    public static DevelopmentProject InitiateNewConstruction(
        LandParcelId landParcelId,
        string name,
        string? description,
        UserId initiatedBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(initiatedBy);

        if (string.IsNullOrWhiteSpace(name))
            throw BusinessRuleViolation.DevelopmentProject.Initiate.NameRequired();

        if (string.IsNullOrWhiteSpace(description))
            throw BusinessRuleViolation.DevelopmentProject.Initiate.DescriptionRequired();

        var project = new DevelopmentProject();

        project.ApplyChange(new DevelopmentProjectInitiated(
            DevelopmentProjectId.Generate().Value,
            landParcelId.Value,
            [],
            name,
            description,
            ProjectType.NewConstruction.Value,
            initiatedBy.Value));

        return project;
    }

    /// <summary>Initiates a new rehabilitation development project for a land parcel.</summary>
    /// <param name="landParcelId">The parcel to associate the project with.</param>    
    /// <param name="name">The name of the development project.</param>
    /// <param name="description">A description of the development project.</param>
    /// <param name="homeIds">The homes associated with the rehabilitation project.</param>
    /// <param name="initiatedBy">The staff user initiating the project.</param>
    public static DevelopmentProject InitiateRehabilitation(
        LandParcelId landParcelId,
        string name,
        string? description,
        IReadOnlyCollection<HomeId> homeIds,
        UserId initiatedBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(initiatedBy);

        if (string.IsNullOrWhiteSpace(name))
            throw BusinessRuleViolation.DevelopmentProject.Initiate.NameRequired();

        if (string.IsNullOrWhiteSpace(description))
            throw BusinessRuleViolation.DevelopmentProject.Initiate.DescriptionRequired();

        if (homeIds == null || homeIds.Count == 0)
            throw BusinessRuleViolation.DevelopmentProject.InitiateRehabilitation.HomeIdsRequired();

        var project = new DevelopmentProject();

        project.ApplyChange(new DevelopmentProjectInitiated(
            DevelopmentProjectId.Generate().Value,
            landParcelId.Value,
            [.. homeIds.Select(h => h.Value)],
            name,
            description,
            ProjectType.Rehabilitation.Value,
            initiatedBy.Value));

        return project;
    }

    /// <inheritdoc/>
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case DevelopmentProjectInitiated e: Apply(e); break;
            default: throw new InvalidOperationException($"Unknown event: {domainEvent?.GetType().Name}");
        }
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
