namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>Summary read model for ground lease template list views.</summary>
public class GroundLeaseTemplateListItemView
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the template name.</summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>Gets or sets the status.</summary>
    public string Status { get; set; } = string.Empty;
}