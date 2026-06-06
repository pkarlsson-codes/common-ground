namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Summary response for a ground lease template list item.</summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="TemplateName">Display name of the template.</param>
/// <param name="Status">Current status (e.g. Active, Inactive).</param>
public sealed record GroundLeaseTemplateListItemResponse(
    string Id,
    string TemplateName,
    string Status);
