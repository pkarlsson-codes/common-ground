using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects;

/// <summary>Status of a development project.</summary>
public sealed class ProjectStatus : Enumeration<ProjectStatus>
{
    /// <summary>Project has been initiated but not yet activated.</summary>
    public static readonly ProjectStatus Initiated = new(1, "Initiated");

    /// <summary>Project is actively being executed.</summary>
    public static readonly ProjectStatus Active = new(2, "Active");

    /// <summary>Project has been completed.</summary>
    public static readonly ProjectStatus Completed = new(3, "Completed");

    /// <summary>Project has been abandoned.</summary>
    public static readonly ProjectStatus Abandoned = new(4, "Abandoned");

    private ProjectStatus(int value, string displayName) : base(value, displayName) { }
}
