using CommonGround.SharedKernel.Domain;

namespace CommonGround.PropertyRegistry.Domain.DevelopmentProjects;

/// <summary>Enumeration of development project types.</summary>
public sealed class ProjectType : Enumeration<ProjectType>
{
    /// <summary>New construction project.</summary>
    public static readonly ProjectType NewConstruction = new(1, "NewConstruction");

    /// <summary>Rehabilitation project.</summary>
    public static readonly ProjectType Rehabilitation = new(2, "Rehabilitation");

    private ProjectType(int value, string name) : base(value, name) { }
}
