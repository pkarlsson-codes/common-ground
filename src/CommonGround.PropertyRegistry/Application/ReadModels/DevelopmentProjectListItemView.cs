namespace CommonGround.PropertyRegistry.Application.ReadModels;

public class DevelopmentProjectListItemView
{
    public string Id { get; set; } = string.Empty;

    public string LandParcelId { get; set; } = string.Empty;

    public IReadOnlyCollection<string> HomeIds { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ProjectType { get; set; } = string.Empty;

    /// <summary>Gets or sets when the project was initiated.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
