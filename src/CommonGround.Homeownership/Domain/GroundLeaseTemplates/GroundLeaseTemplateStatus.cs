using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.GroundLeaseTemplates;

/// <summary>
/// Status of a ground lease template.
/// </summary>
public class GroundLeaseTemplateStatus : Enumeration<GroundLeaseTemplateStatus>
{
    /// <summary>The template is active and can be used to create leases.</summary>
    public static readonly GroundLeaseTemplateStatus Active = new(1, nameof(Active));
       
    /// <summary>The template is inactive and cannot be used to create leases.</summary>
    public static readonly GroundLeaseTemplateStatus Inactive = new(2, nameof(Inactive));

    private GroundLeaseTemplateStatus(int value, string name) : base(value, name)
    {
    }
}
