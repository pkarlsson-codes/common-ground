namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>Summary read model for development project list displays.</summary>
public class DevelopmentProjectListItemView
{
    /// <summary>Gets or sets the project ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the land parcel ID.</summary>
    public string LandParcelId { get; set; } = string.Empty;

    /// <summary>Gets or sets the home IDs associated with this project.</summary>
    public IReadOnlyCollection<string> HomeIds { get; set; } = [];

    /// <summary>Gets or sets the project name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the project description.</summary>
    public string? Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Gets or sets the project type.</summary>
    public string ProjectType { get; set; } = string.Empty;

    /// <summary>Gets or sets when the project was initiated.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
