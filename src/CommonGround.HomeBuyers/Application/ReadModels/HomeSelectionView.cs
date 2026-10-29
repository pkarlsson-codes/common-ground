namespace CommonGround.HomeBuyers.Application.ReadModels;

/// <summary>Read model for a home selection; one doc per selection, queried by home.</summary>
public class HomeSelectionView
{
    /// <summary>Gets or sets the selection ID (the identity of this view).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the home being selected for.</summary>
    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets the selection status.</summary>
    public string Status { get; set; } = string.Empty;
}
