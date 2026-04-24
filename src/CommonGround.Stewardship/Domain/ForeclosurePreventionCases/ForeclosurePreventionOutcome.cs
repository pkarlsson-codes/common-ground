namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>The outcome of a resolved foreclosure prevention case.</summary>
public enum ForeclosurePreventionOutcome
{
    /// <summary>CLT intervention was successful and the homeowner retained the home.</summary>
    InterventionSuccessful = 0,

    /// <summary>The homeowner sold the home through the resale process.</summary>
    HomeownerSold = 1,

    /// <summary>A cure period was exercised to resolve the delinquency.</summary>
    CurePeriodExercised = 2,

    /// <summary>The ground lease was terminated.</summary>
    LeaseTerminated = 3
}
