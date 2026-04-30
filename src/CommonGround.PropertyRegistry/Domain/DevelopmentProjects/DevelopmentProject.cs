using CommonGround.PropertyRegistry.Domain.DevelopmentProjects.Events;
using CommonGround.PropertyRegistry.Domain.LandParcels;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;
using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects;

/// <summary>
/// A development project associated with a CLT land parcel.
/// Tracks the intent and status of development activity on a parcel.
/// </summary>
public sealed class DevelopmentProject : AggregateRoot<DevelopmentProjectId>
{
#pragma warning disable CS8618
    private DevelopmentProject() { }
#pragma warning restore CS8618

    /// <summary>Gets the ID of the land parcel this project is for.</summary>
    public LandParcelId LandParcelId { get; private set; }

    /// <summary>Gets the description of the development project.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the current status of this project.</summary>
    public ProjectStatus ProjectStatus { get; private set; }

    /// <summary>Initiates a new development project for a land parcel.</summary>
    public static DevelopmentProject Initiate(
        LandParcelId landParcelId,
        string description,
        UserId initiatedBy)
    {
        ArgumentNullException.ThrowIfNull(landParcelId);
        ArgumentNullException.ThrowIfNull(initiatedBy);

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException("Development project description must be specified.");

        var project = new DevelopmentProject();

        project.ApplyChange(new DevelopmentProjectInitiated(
            DevelopmentProjectId.Generate().Value,
            landParcelId.Value,
            description,
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
        Description = e.Description;
        ProjectStatus = ProjectStatus.Initiated;
    }
}
