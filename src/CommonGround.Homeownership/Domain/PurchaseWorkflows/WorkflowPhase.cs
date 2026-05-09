using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.PurchaseWorkflows;

/// <summary>High-level lifecycle state of a purchase workflow.</summary>
public sealed class WorkflowPhase : Enumeration<WorkflowPhase>
{
    /// <summary>Workflow has been initiated, but no substantive progress yet.</summary>
    public static readonly WorkflowPhase Initiated = new(0, nameof(Initiated));

    /// <summary>At least one track has been satisfied.</summary>
    public static readonly WorkflowPhase InProgress = new(1, nameof(InProgress));

    /// <summary>All mandatory tracks satisfied, closing date set.</summary>
    public static readonly WorkflowPhase ClosingScheduled = new(2, nameof(ClosingScheduled));

    /// <summary>Deal complete, ownership finalized.</summary>
    public static readonly WorkflowPhase Closed = new(3, nameof(Closed));

    /// <summary>Deal terminated with specific reason.</summary>
    public static readonly WorkflowPhase Failed = new(4, nameof(Failed));

    /// <summary>Buyer explicitly withdrew.</summary>
    public static readonly WorkflowPhase Withdrawn = new(5, nameof(Withdrawn));

    /// <summary>Process paused, recoverable by recording next fact.</summary>
    public static readonly WorkflowPhase Stalled = new(6, nameof(Stalled));

    /// <summary>Indicates whether the workflow phase is terminal.</summary>
    public bool IsTerminal => this == Closed || this == Failed || this == Withdrawn;

    /// <summary>Indicates whether the workflow can advance to In Progress.</summary>
    public bool CanAdvanceToInProgress => this == Initiated || this == Stalled;

    private WorkflowPhase(int value, string name) : base(value, name)
    {
    }
}
