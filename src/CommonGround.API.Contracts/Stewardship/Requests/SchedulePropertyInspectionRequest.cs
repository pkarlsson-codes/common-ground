namespace CommonGround.API.Contracts.Stewardship.Requests;

/// <summary>Request to schedule a property inspection on a stewardship case.</summary>
public sealed record SchedulePropertyInspectionRequest(
    DateOnly ScheduledDate,
    string InspectorName);
