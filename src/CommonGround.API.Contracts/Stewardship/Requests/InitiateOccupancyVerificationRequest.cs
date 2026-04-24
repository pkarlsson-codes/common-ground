namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to initiate an annual occupancy verification.</summary>
public sealed record InitiateOccupancyVerificationRequest(
    int PeriodYear,
    DateOnly? SurveyDeadline);
