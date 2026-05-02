namespace CommonGround.Homeownership.Domain.GroundLeases.Templates;

/// <summary>
/// Status of a ground lease template.
/// </summary>
public enum GroundLeaseTemplateStatus
{
    /// <summary>The template is active and can be used to create leases.</summary>
    Active = 0,

    /// <summary>The template is inactive and cannot be used to create leases.</summary>
    Inactive = 1
}
