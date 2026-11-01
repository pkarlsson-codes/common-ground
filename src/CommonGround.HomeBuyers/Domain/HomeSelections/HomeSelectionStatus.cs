using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>Where a home selection is in its lifecycle.</summary>
public sealed class HomeSelectionStatus : Enumeration<HomeSelectionStatus>
{
    /// <summary>Candidates are snapshotted and staff have not yet chosen a household.</summary>
    public static readonly HomeSelectionStatus Open = new(0, nameof(Open));
    /// <summary>A household was chosen, by priority or by lottery.</summary>
    public static readonly HomeSelectionStatus Completed = new(1, nameof(Completed));
    /// <summary>Staff cancelled the selection without choosing anyone.</summary>
    public static readonly HomeSelectionStatus Cancelled = new(2, nameof(Cancelled));
    /// <summary>A household was chosen, but the purchase it led to failed or was withdrawn.</summary>
    public static readonly HomeSelectionStatus FellThrough = new(3, nameof(FellThrough));

    private HomeSelectionStatus(int value, string displayName) : base(value, displayName) { }
}
