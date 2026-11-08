namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record the appraisal the lease requires after a homeowner's notice to sell.</summary>
/// <param name="AppraisedValue">The home's appraised market value.</param>
/// <param name="AppraisalDate">The day of the appraisal.</param>
/// <param name="AppraiserName">Who appraised the home.</param>
/// <param name="ApprovedImprovements">The approved improvements the lease's formula credits, if it credits any.</param>
/// <param name="CurrentIndexValue">The current index value, when the lease's formula is index based.</param>
public sealed record RecordResaleAppraisalRequest(
    decimal AppraisedValue,
    DateOnly AppraisalDate,
    string AppraiserName,
    decimal? ApprovedImprovements,
    decimal? CurrentIndexValue);
