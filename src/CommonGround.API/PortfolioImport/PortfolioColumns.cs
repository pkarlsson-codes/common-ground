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

    /// <summary>
    /// The column whose value breaks each domain rule the import checks, by error code. A rule not listed here is
    /// reported on the row as a whole.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> ByErrorCode = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["property_registry.land_parcel.register.legal_parcel_number_required"] = ParcelNumber,
        ["property_registry.land_parcel.register.address_required"] = ParcelAddress,
        ["property_registry.land_parcel.register.area_non_positive"] = ParcelAcres,
        ["property_registry.home.register.bedrooms_negative"] = Bedrooms,
        ["property_registry.home.register.bathrooms_non_positive"] = Bathrooms,
        ["property_registry.home.register.square_footage_non_positive"] = SquareFeet,
        ["property_registry.home.register.year_built_out_of_range"] = YearBuilt,
        ["property_registry.unit_assessment.create.assessed_value_negative"] = AssessedValue,
        ["property_registry.unit_assessment.create.assessment_date_in_future"] = AssessedOn,
        ["shared.person_name.required"] = HomeownerName,
        ["shared.person_name.too_long"] = HomeownerName,
        ["shared.email.required"] = HomeownerEmail,
        ["shared.email.invalid_format"] = HomeownerEmail,
        ["shared.phone.invalid_format"] = HomeownerPhone,
        ["homeownership.ground_lease.create.base_price_non_positive"] = BasePrice,
        ["homeownership.ground_lease.create.original_appraised_value_non_positive"] = AppraisedValue,
        ["homeownership.lease_fee_terms.create.land_use_fee_negative"] = LandUseFee,
        ["homeownership.lease_fee_terms.create.repair_reserve_fee_negative"] = RepairReserveFee,
        ["homeownership.lease_fee_tracking.create.tracked_from_before_lease_start"] = FeesTrackedFrom,
        ["homeownership.lease_fee_tracking.create.opening_balance_negative"] = AmountOwed,
    };
}
