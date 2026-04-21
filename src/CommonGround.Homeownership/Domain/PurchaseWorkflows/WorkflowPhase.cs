namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>High-level lifecycle state of a purchase workflow.</summary>
public enum WorkflowPhase
{
    /// <summary>Workflow just created, no tracks satisfied yet.</summary>
    Initiated = 0,

    /// <summary>At least one track has been satisfied.</summary>
    InProgress = 1,

    /// <summary>All mandatory tracks satisfied, closing date set.</summary>
    ClosingScheduled = 2,

    /// <summary>Deal complete, ownership finalized.</summary>
    Closed = 3,

    /// <summary>Deal terminated with specific reason.</summary>
    Failed = 4,

    /// <summary>Buyer explicitly withdrew.</summary>
    Withdrawn = 5,

    /// <summary>Process paused, recoverable by recording next fact.</summary>
    Stalled = 6
}
