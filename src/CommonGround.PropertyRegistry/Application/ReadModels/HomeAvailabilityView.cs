namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>
/// Where a home stands in being sold, pieced together from HomeBuyers' selections and Homeownership's
/// purchases. Those contexts decide; this only shows what they decided, so it takes their news in whatever
/// order it arrives and never refuses any of it. A home without one is available.
/// </summary>
public class HomeAvailabilityView
{
    /// <summary>Gets or sets the home ID (the identity of this view).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the home's status display name.</summary>
    public string Status { get; set; } = HomeStatus.Available.DisplayName;

    /// <summary>Gets or sets the date the home last became available again after its purchase fell through, if it has.</summary>
    public DateOnly? AvailableAgainDate { get; set; }

    /// <summary>Gets or sets the date the home's current selection opened, while it is in selection or later.</summary>
    public DateOnly? SelectionOpenedDate { get; set; }

    /// <summary>Gets or sets the date the home went under contract, while it is under contract or later.</summary>
    public DateOnly? UnderContractDate { get; set; }

    /// <summary>Gets or sets the date the home's purchase closed and it was leased.</summary>
    public DateOnly? LeasedDate { get; set; }

    /// <summary>Gets or sets the home's selections, as far as news of them has arrived.</summary>
    public List<HomeAvailabilitySelection> Selections { get; set; } = [];

    /// <summary>
    /// Gets or sets purchases that fell through before news arrived of the selection that led to them,
    /// by the application that was selected.
    /// </summary>
    public Dictionary<string, DateOnly> FellThroughBeforeSelected { get; set; } = [];

    /// <summary>Records that a selection started choosing a household for the home.</summary>
    public void RecordSelectionStarted(string selectionId, DateTimeOffset startedAt, DateOnly openedDate)
    {
        var selection = SelectionFor(selectionId);
        selection.StartedAt = startedAt;
        selection.OpenedDate = openedDate;
        Refresh();
    }

    /// <summary>Records that a selection was cancelled without choosing anyone.</summary>
    public void RecordSelectionCancelled(string selectionId)
    {
        SelectionFor(selectionId).Cancelled = true;
        Refresh();
    }

    /// <summary>Records that a selection chose a household, putting the home under contract.</summary>
    public void RecordApplicationSelected(string selectionId, string applicationId, DateOnly selectedDate)
    {
        var selection = SelectionFor(selectionId);
        selection.ApplicationId = applicationId;
        selection.SelectedDate = selectedDate;
        if (FellThroughBeforeSelected.Remove(applicationId, out var fellThroughDate))
            selection.FellThroughDate = fellThroughDate;

        Refresh();
    }

    /// <summary>
    /// Records that a purchase fell through. One started directly, rather than from a selection, has no
    /// application and never showed the home under contract, so it changes nothing.
    /// </summary>
    public void RecordPurchaseFellThrough(string? applicationId, DateOnly fellThroughDate)
    {
        if (applicationId is null)
            return;

        var selection = Selections.Find(s => s.ApplicationId == applicationId);
        if (selection is null)
            FellThroughBeforeSelected[applicationId] = fellThroughDate;
        else
            selection.FellThroughDate = fellThroughDate;

        Refresh();
    }

    /// <summary>Records that the home's purchase closed, leasing it to its homeowner.</summary>
    public void RecordPurchaseClosed(DateOnly closingDate)
    {
        LeasedDate = closingDate;
        Refresh();
    }

    private HomeAvailabilitySelection SelectionFor(string selectionId)
    {
        var selection = Selections.Find(s => s.Id == selectionId);
        if (selection is null)
        {
            selection = new HomeAvailabilitySelection { Id = selectionId };
            Selections.Add(selection);
        }

        return selection;
    }

    // HomeBuyers runs one selection per home at a time, so the latest to start is the one that counts;
    // a selection heard of only through its outcome counts once news of its start arrives.
    private void Refresh()
    {
        var latest = Selections.Where(s => s.StartedAt is not null).MaxBy(s => s.StartedAt);
        var active = latest is { Cancelled: false, FellThroughDate: null } ? latest : null;

        Status = (LeasedDate, active) switch
        {
            ({ }, _) => HomeStatus.Leased.DisplayName,
            (null, null) => HomeStatus.Available.DisplayName,
            (null, { ApplicationId: null }) => HomeStatus.InSelection.DisplayName,
            _ => HomeStatus.UnderContract.DisplayName,
        };
        SelectionOpenedDate = active?.OpenedDate;
        UnderContractDate = active?.SelectedDate;
        AvailableAgainDate = Selections.Max(s => s.FellThroughDate);
    }
}

/// <summary>What a <see cref="HomeAvailabilityView"/> has heard of one of the home's selections.</summary>
public class HomeAvailabilitySelection
{
    /// <summary>Gets or sets the selection ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets when the selection started, once news of it has arrived.</summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>Gets or sets the trust date the selection started on.</summary>
    public DateOnly? OpenedDate { get; set; }

    /// <summary>Gets or sets a value indicating whether the selection was cancelled without choosing anyone.</summary>
    public bool Cancelled { get; set; }

    /// <summary>Gets or sets the application the selection chose, once it has.</summary>
    public string? ApplicationId { get; set; }

    /// <summary>Gets or sets the date the selection chose a household.</summary>
    public DateOnly? SelectedDate { get; set; }

    /// <summary>Gets or sets the date the purchase the selection led to fell through, once it has.</summary>
    public DateOnly? FellThroughDate { get; set; }
}
