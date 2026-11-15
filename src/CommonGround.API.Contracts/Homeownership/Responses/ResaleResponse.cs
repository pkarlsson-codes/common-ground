namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>A resale, from the homeowner's notice of intent to sell.</summary>
/// <param name="Id">The resale ID.</param>
/// <param name="GroundLeaseId">The lease of the homeowner selling.</param>
/// <param name="LandParcelId">The leased land parcel.</param>
/// <param name="HomeId">The home being sold.</param>
/// <param name="HomeLabel">The label staff know the home by, once known.</param>
/// <param name="HomeownerFullName">The selling homeowner's full name.</param>
/// <param name="Status">Open, Withdrawn or Closed.</param>
/// <param name="NoticeDate">The day the homeowner gave notice.</param>
/// <param name="NoticeChannel">How the notice reached the trust.</param>
/// <param name="Notes">Anything staff noted about the notice.</param>
/// <param name="OptionWindowEndsOn">The last day of the trust's purchase option.</param>
/// <param name="WithdrawnDate">The day the homeowner withdrew, if they did.</param>
/// <param name="WithdrawnReason">Why the homeowner withdrew, if they did.</param>
/// <param name="RecordedAt">When the notice was recorded.</param>
/// <param name="AppraisedValue">The latest appraisal's value, once one is recorded.</param>
/// <param name="AppraisalDate">The day of the latest appraisal.</param>
/// <param name="AppraiserName">Who made the latest appraisal.</param>
/// <param name="ApprovedImprovements">The approved improvements the latest appraisal credited, if any.</param>
/// <param name="CurrentIndexValue">The current index value the latest appraisal used, if any.</param>
/// <param name="FormulaPrice">The most the lease's formula allows the home to sell for.</param>
/// <param name="PurchaseOptionPrice">What the trust pays to buy the home.</param>
/// <param name="OptionExercisedDate">The day the trust decided to buy the home, if it has.</param>
/// <param name="OptionExercisedNotes">Anything staff noted about the decision.</param>
/// <param name="ClosingDate">The day the buyback closed, if it has.</param>
/// <param name="PricePaid">What the trust paid at the closing.</param>
/// <param name="ClosingNotes">Anything staff noted about the closing.</param>
/// <param name="OfferedForSale">Whether the home was offered to the waiting list at the closing, rather than held.</param>
/// <param name="HoldReason">Why the trust is holding the home, if it is.</param>
/// <param name="FeesPaidFromProceeds">Lease fees paid to the trust from the seller's proceeds at the closing, if any.</param>
/// <param name="PayoffStatements">The payoff statements printed for the closing agent, oldest first.</param>
public sealed record ResaleResponse(
    string Id,
    string GroundLeaseId,
    string LandParcelId,
    string HomeId,
    string? HomeLabel,
    string? HomeownerFullName,
    string Status,
    DateOnly NoticeDate,
    string NoticeChannel,
    string? Notes,
    DateOnly OptionWindowEndsOn,
    DateOnly? WithdrawnDate,
    string? WithdrawnReason,
    DateTimeOffset RecordedAt,
    decimal? AppraisedValue,
    DateOnly? AppraisalDate,
    string? AppraiserName,
    decimal? ApprovedImprovements,
    decimal? CurrentIndexValue,
    decimal? FormulaPrice,
    decimal? PurchaseOptionPrice,
    DateOnly? OptionExercisedDate,
    string? OptionExercisedNotes,
    DateOnly? ClosingDate,
    decimal? PricePaid,
    string? ClosingNotes,
    bool? OfferedForSale,
    string? HoldReason,
    decimal? FeesPaidFromProceeds,
    IReadOnlyList<PayoffStatementPrintDto> PayoffStatements);

/// <summary>A payoff statement printed for a resale's closing agent.</summary>
/// <param name="PrintedAt">When it was printed.</param>
/// <param name="PrintedBy">The staff user who printed it.</param>
/// <param name="GoodThrough">The day the figures were worked out for.</param>
/// <param name="TotalOwed">The amount the letter asked for.</param>
public sealed record PayoffStatementPrintDto(DateTimeOffset PrintedAt, string PrintedBy, DateOnly GoodThrough, decimal TotalOwed);

/// <summary>What a resale's seller owes the trust as of a day, for the closing agent to pay from proceeds.</summary>
/// <param name="GoodThrough">The day the figures are worked out for.</param>
/// <param name="FeesOwed">Lease fees owed, less payments; zero if paid up or over.</param>
/// <param name="InterestOwed">Interest owed after waivers.</param>
/// <param name="Credit">What was paid beyond the fees charged; zero if none.</param>
/// <param name="TotalOwed">Fees and interest less credit, never below zero.</param>
/// <param name="OldestUnpaidMonth">The due date of the oldest unpaid fee, or null when none is unpaid.</param>
public sealed record PayoffStatementResponse(
    DateOnly GoodThrough,
    decimal FeesOwed,
    decimal InterestOwed,
    decimal Credit,
    decimal TotalOwed,
    DateOnly? OldestUnpaidMonth);
