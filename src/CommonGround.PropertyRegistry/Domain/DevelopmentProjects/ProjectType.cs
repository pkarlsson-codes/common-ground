using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects;

/// <summary>Enumeration of development project types.</summary>
public sealed class ProjectType : Enumeration<ProjectType>
{
    /// <summary>New construction project.</summary>
    public static readonly ProjectType NewConstruction = new(1, "New Construction");

    /// <summary>Rehabilitation project.</summary>
    public static readonly ProjectType Rehabilitation = new(2, "Renovation");

    private ProjectType(int value, string name) : base(value, name) { }
}
