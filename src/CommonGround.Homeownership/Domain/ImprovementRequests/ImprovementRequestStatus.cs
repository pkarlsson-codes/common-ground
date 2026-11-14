using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.ImprovementRequests;

/// <summary>Where a homeowner's request to alter or add to the home stands.</summary>
public sealed class ImprovementRequestStatus : Enumeration<ImprovementRequestStatus>
{
    /// <summary>Received and waiting for the trust's reply.</summary>
    public static readonly ImprovementRequestStatus Waiting = new(1, "Waiting for decision");

    /// <summary>The trust approved the work and pre-approved a credit; the work isn't recorded finished.</summary>
    public static readonly ImprovementRequestStatus Approved = new(2, "Approved");

    /// <summary>The trust refused the work.</summary>
    public static readonly ImprovementRequestStatus Denied = new(3, "Denied");

    /// <summary>The work is finished and the credit confirmed; it counts toward the lease's improvements.</summary>
    public static readonly ImprovementRequestStatus Completed = new(4, "Completed");

    /// <summary>The homeowner dropped the work.</summary>
    public static readonly ImprovementRequestStatus Withdrawn = new(5, "Withdrawn");

    private ImprovementRequestStatus(int value, string displayName) : base(value, displayName) { }
}
