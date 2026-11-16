namespace CommonGround.Homeownership.Application.ReadModels;

/// <summary>
/// Homeownership's local copy of one of HomeBuyers' home selections, so a lease isn't created by hand for a
/// home that is being offered to the waiting list. Kept (eventually consistent) from HomeBuyers'
/// HomeSelectionStarted, HomeSelectionCancelled and ApplicationSelected integration events, in whatever
/// order they arrive.
/// </summary>
public class HomeSelectionView
{
    /// <summary>Gets or sets the selection ID (the identity of this view).</summary>
    public string Id { get; set; } = string.Empty;

    public string HomeId { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether news of the selection's start has arrived.</summary>
    public bool Started { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the selection has ended, by being cancelled or by choosing a
    /// household (whose purchase then stands for the home).
    /// </summary>
    public bool Ended { get; set; }

    /// <summary>Gets or sets a value indicating whether the selection is still choosing a household.</summary>
    public bool Open { get; set; }

    public void RecordStarted()
    {
        Started = true;
        Open = !Ended;
    }

    public void RecordEnded()
    {
        Ended = true;
        Open = false;
    }
}
