namespace CommonGround.API.Contracts.Homeownership.Requests;

/// <summary>Request to record that a late lease fee notice was sent.</summary>
/// <param name="Level">Which notice: First, Second or Default.</param>
/// <param name="SentOn">The day it was sent.</param>
/// <param name="Delivery">How it was sent: FirstClassMail, CertifiedMail, RegisteredMail, HandDelivered or Email.</param>
public sealed record RecordLeaseFeeNoticeRequest(
    string Level,
    DateOnly SentOn,
    string Delivery);

/// <summary>Request to set the trust's lease fee notice policy.</summary>
/// <param name="FirstNoticeDays">Days overdue before the first notice; at least 1.</param>
/// <param name="SecondNoticeDays">Days overdue before the second notice; more than the first.</param>
/// <param name="DefaultNoticeDays">Days overdue before the notice of default; more than the second.</param>
/// <param name="CureDays">Days a notice of default gives to pay; at least 1.</param>
/// <param name="CureExtensionDays">Days added when two-thirds is paid within the cure days; zero for none.</param>
/// <param name="LetterSignOff">The lines printed at the end of every letter; empty or null for none.</param>
public sealed record SetLeaseFeeNoticePolicyRequest(
    int FirstNoticeDays,
    int SecondNoticeDays,
    int DefaultNoticeDays,
    int CureDays,
    int CureExtensionDays,
    string? LetterSignOff);

/// <summary>Request to waive late-fee interest owed on a lease today.</summary>
/// <param name="Amount">How much interest to waive; no more than is owed.</param>
/// <param name="Reason">Why, such as hardship.</param>
public sealed record WaiveLeaseFeeInterestRequest(
    decimal Amount,
    string Reason);
