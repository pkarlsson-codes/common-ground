namespace CommonGround.API.Contracts.Demo.Responses;

/// <summary>What the demo banner shows for the current trust.</summary>
/// <param name="DeletesOn">The date the trust is deleted, in the trust's time zone.</param>
/// <param name="FeedbackUrl">Where "Send feedback" goes: a <c>mailto:</c> link or a form's web address.</param>
public sealed record DemoResponse(DateOnly DeletesOn, string FeedbackUrl);
