using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.HeirTransfers;

/// <summary>How the heir qualifies under Model Lease Art 10.3.</summary>
public sealed class HeirRelationship : Enumeration<HeirRelationship>
{
    /// <summary>The homeowner's spouse.</summary>
    public static readonly HeirRelationship Spouse = new(1, "Spouse");

    /// <summary>The homeowner's child.</summary>
    public static readonly HeirRelationship Child = new(2, "Child");

    /// <summary>A member of the homeowner's household who lived in the home for the year before the death.</summary>
    public static readonly HeirRelationship HouseholdMember = new(3, "HouseholdMember");

    /// <summary>Any other heir, who must be income-qualified.</summary>
    public static readonly HeirRelationship Other = new(4, "Other");

    private HeirRelationship(int value, string displayName) : base(value, displayName) { }
}
