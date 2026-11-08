namespace CommonGround.PropertyRegistry.Application.ReadModels;

/// <summary>
/// Where a home stands in being sold, pieced together from HomeBuyers' selections and Homeownership's
/// purchases, leases and resales. Those contexts decide; this only shows what they decided, so it takes their news in whatever
/// order it arrives and never refuses any of it. A home without one is available.
/// </summary>
public class HomeAvailabilityView
{
    /// <summary>Gets or sets the home ID (the identity of this view).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the home's status display name.</summary>
    public string Status { get; set; } = HomeStatus.Available.DisplayName;

    /// <summary>Gets or sets who has the home now: the trust (vacant) or a homeowner (occupied), as a <see cref="HomeOccupancy"/> display name.</summary>
    public string Occupancy { get; set; } = HomeOccupancy.Vacant.DisplayName;

    /// <summary>Gets or sets where the home is in being sold, as a <see cref="HomeSaleStage"/> display name.</summary>
    public string SaleStage { get; set; } = HomeSaleStage.Offered.DisplayName;

    /// <summary>Gets or sets the date the home last became available again after its purchase fell through, if it has.</summary>
    public DateOnly? AvailableAgainDate { get; set; }

    /// <summary>Gets or sets the date the home's current selection opened, while it is in selection or later.</summary>
    public DateOnly? SelectionOpenedDate { get; set; }

    /// <summary>Gets or sets the date the home went under contract, while it is under contract or later.</summary>
    public DateOnly? UnderContractDate { get; set; }

    /// <summary>Gets or sets the start date of the home's ground lease, once it has one.</summary>
    public DateOnly? LeasedDate { get; set; }

    /// <summary>Gets or sets the day the homeowner gave notice to sell, while a resale of the home is open.</summary>
    public DateOnly? ResaleNoticeDate { get; set; }

    /// <summary>Gets or sets the home's selections, as far as news of them has arrived.</summary>
    public List<HomeAvailabilitySelection> Selections { get; set; } = [];

    /// <summary>Gets or sets the home's resales, as far as news of them has arrived.</summary>
    public List<HomeAvailabilityResale> Resales { get; set; } = [];

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

    /// <summary>Records that a ground lease was created for the home, from its purchase closing or directly.</summary>
    public void RecordLeased(DateOnly leaseStartDate)
    {
        LeasedDate = leaseStartDate;
        Refresh();
    }

    /// <summary>Records that the homeowner gave notice to sell, opening a resale of the home.</summary>
    public void RecordResaleOpened(string resaleId, DateOnly noticeDate)
    {
        ResaleFor(resaleId).NoticeDate = noticeDate;
        Refresh();
    }

    /// <summary>Records that the homeowner withdrew their notice to sell.</summary>
    public void RecordResaleWithdrawn(string resaleId)
    {
        ResaleFor(resaleId).Withdrawn = true;
        Refresh();
    }

    private HomeAvailabilityResale ResaleFor(string resaleId)
    {
        var resale = Resales.Find(r => r.Id == resaleId);
        if (resale is null)
        {
            resale = new HomeAvailabilityResale { Id = resaleId };
            Resales.Add(resale);
        }

        return resale;
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

    // Status is the one label the list, filters and stepper still read; each sale stage maps to one status today.
    private static string StatusFor(string saleStage)
        => HomeSaleStage.FromDisplayName(saleStage) switch
        {
            var s when s == HomeSaleStage.AwaitingTrustDecision => HomeStatus.Resale.DisplayName,
            var s when s == HomeSaleStage.NotForSale => HomeStatus.Leased.DisplayName,
            var s when s == HomeSaleStage.InSelection => HomeStatus.InSelection.DisplayName,
            var s when s == HomeSaleStage.UnderContract => HomeStatus.UnderContract.DisplayName,
            _ => HomeStatus.Available.DisplayName,
        };

    // HomeBuyers runs one selection per home at a time, so the latest to start is the one that counts;
    // a selection heard of only through its outcome counts once news of its start arrives. A lease has one
    // open resale at a time, so one heard of and not withdrawn is it, whether or not news of the lease came first.
    private void Refresh()
    {
        var latest = Selections.Where(s => s.StartedAt is not null).MaxBy(s => s.StartedAt);
        var active = latest is { Cancelled: false, FellThroughDate: null } ? latest : null;
        var openResale = Resales.Find(r => r is { NoticeDate: not null, Withdrawn: false });

        Occupancy = (LeasedDate is null ? HomeOccupancy.Vacant : HomeOccupancy.Occupied).DisplayName;
        var saleStage = (LeasedDate, active) switch
        {
            _ when openResale is not null => HomeSaleStage.AwaitingTrustDecision,
            ({ }, _) => HomeSaleStage.NotForSale,
            (null, null) => HomeSaleStage.Offered,
            (null, { ApplicationId: null }) => HomeSaleStage.InSelection,
            _ => HomeSaleStage.UnderContract,
        };
        SaleStage = saleStage.DisplayName;
        Status = StatusFor(SaleStage);
        SelectionOpenedDate = active?.OpenedDate;
        UnderContractDate = active?.SelectedDate;
        AvailableAgainDate = Selections.Max(s => s.FellThroughDate);
        ResaleNoticeDate = openResale?.NoticeDate;
    }
}

/// <summary>What a <see cref="HomeAvailabilityView"/> has heard of one of the home's resales.</summary>
public class HomeAvailabilityResale
{
    /// <summary>Gets or sets the resale ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the day the homeowner gave notice, once news of the resale opening has arrived.</summary>
    public DateOnly? NoticeDate { get; set; }

    /// <summary>Gets or sets a value indicating whether the homeowner withdrew their notice.</summary>
    public bool Withdrawn { get; set; }
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
