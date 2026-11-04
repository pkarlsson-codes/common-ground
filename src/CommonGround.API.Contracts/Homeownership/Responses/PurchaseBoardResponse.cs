namespace CommonGround.API.Contracts.Homeownership.Responses;

/// <summary>Every open purchase, in one column per step it waits on.</summary>
/// <param name="Today">The trust's today, to count days on a step against.</param>
/// <param name="Columns">One column per step, in step order, empty ones included.</param>
public sealed record PurchaseBoardResponse(DateOnly Today, IReadOnlyList<PurchaseBoardColumnResponse> Columns);

/// <summary>The purchases waiting on one step, longest waiting first.</summary>
/// <param name="Step">The step: Eligibility, Financing, Inspection, Title, LeaseSigning, or Closing.</param>
/// <param name="Cards">The purchases waiting on it.</param>
public sealed record PurchaseBoardColumnResponse(string Step, IReadOnlyList<PurchaseBoardCardResponse> Cards);

/// <summary>An open purchase on the board.</summary>
/// <param name="Id">The purchase workflow ID.</param>
/// <param name="HomeId">The home being bought.</param>
/// <param name="HomeLabel">The home's label, if known.</param>
/// <param name="BuyerFullName">The buyer's full name.</param>
/// <param name="Phase">The workflow phase, e.g. Stalled.</param>
/// <param name="StepEnteredOn">The trust date the purchase last moved on.</param>
/// <param name="ScheduledClosingDate">The scheduled closing date, once closing is scheduled.</param>
public sealed record PurchaseBoardCardResponse(
    string Id,
    string HomeId,
    string? HomeLabel,
    string BuyerFullName,
    string Phase,
    DateOnly StepEnteredOn,
    DateOnly? ScheduledClosingDate);
