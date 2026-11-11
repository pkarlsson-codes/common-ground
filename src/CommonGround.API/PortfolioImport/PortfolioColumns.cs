namespace CommonGround.API.PortfolioImport;

/// <summary>The column headers of a portfolio import file. Headers match ignoring case; column order doesn't matter.</summary>
#pragma warning disable SA1600 // The constant names are the headers.
internal static class PortfolioColumns
{
    public const string ParcelNumber = "Parcel Number";
    public const string ParcelAddress = "Parcel Address";
    public const string ParcelAcres = "Parcel Acres";
    public const string AcquiredOn = "Acquired On";
    public const string AcquiredBy = "Acquired By";
    public const string ParcelCovenant = "Parcel Covenant";
    public const string Unit = "Unit";
    public const string HomeType = "Home Type";
    public const string Bedrooms = "Bedrooms";
    public const string Bathrooms = "Bathrooms";
    public const string Accessible = "Accessible";
    public const string SquareFeet = "Square Feet";
    public const string YearBuilt = "Year Built";
    public const string AssessedValue = "Assessed Value";
    public const string AssessedOn = "Assessed On";
    public const string HomeownerName = "Homeowner Name";
    public const string HomeownerEmail = "Homeowner Email";
    public const string HomeownerPhone = "Homeowner Phone";
    public const string LeaseTemplate = "Lease Template";
    public const string LeaseStart = "Lease Start";
    public const string BasePrice = "Base Price";
    public const string AppraisedValue = "Appraised Value at Purchase";
    public const string LandUseFee = "Land Use Fee";
    public const string RepairReserveFee = "Repair Reserve Fee";
    public const string FeesTrackedFrom = "Fees Tracked From";
    public const string AmountOwed = "Amount Owed Then";

#pragma warning restore SA1600

    /// <summary>The headers a file must have. The others may be left out.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        ParcelNumber, ParcelAddress, ParcelAcres, AcquiredOn, AcquiredBy,
        HomeType, Bedrooms, Bathrooms, SquareFeet, YearBuilt, AssessedValue, AssessedOn,
    ];
}
