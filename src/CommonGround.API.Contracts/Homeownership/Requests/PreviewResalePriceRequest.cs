namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request for the prices an appraisal would set, without recording it.</summary>
/// <param name="AppraisedValue">The home's appraised market value.</param>
/// <param name="AppraisalDate">The day of the appraisal.</param>
/// <param name="ApprovedImprovements">The approved improvements the lease's formula credits, if it credits any.</param>
/// <param name="CurrentIndexValue">The current index value, when the lease's formula is index based.</param>
public sealed record PreviewResalePriceRequest(
    decimal AppraisedValue,
    DateOnly AppraisalDate,
    decimal? ApprovedImprovements,
    decimal? CurrentIndexValue);
