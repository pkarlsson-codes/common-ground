namespace CommonGround.API.Contracts.Demo.Requests;

/// <summary>Feedback a visitor sends from the demo banner.</summary>
/// <param name="Message">What they want to say; not empty, at most 4,000 characters.</param>
/// <param name="Page">The page they were on, such as <c>/resales/resale-01…</c>; cut at 500 characters.</param>
/// <param name="MayReply">Whether the operator may reply to the visitor's sign-in email address.</param>
public sealed record SendDemoFeedbackRequest(string Message, string? Page, bool MayReply);
