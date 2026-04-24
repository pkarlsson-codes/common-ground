namespace CommonGround.Stewardship.Domain.ForeclosurePreventionCases;

/// <summary>The event that triggered opening a foreclosure prevention case.</summary>
public enum ForeclosurePreventionTrigger
{
    /// <summary>Triggered by lease fee arrears escalation.</summary>
    LeaseFeeArrears = 0,

    /// <summary>Triggered by a manually recorded servicer delinquency.</summary>
    ServicerDelinquency = 1
}
