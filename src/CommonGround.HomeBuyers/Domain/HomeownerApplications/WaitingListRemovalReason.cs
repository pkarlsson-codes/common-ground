using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeownerApplications;

/// <summary>Why the trust took an approved household off the waiting list.</summary>
public sealed class WaitingListRemovalReason : Enumeration<WaitingListRemovalReason>
{
    /// <summary>The household's income, size or other eligibility changed.</summary>
    public static readonly WaitingListRemovalReason NoLongerEligible = new(1, nameof(NoLongerEligible));

    /// <summary>The trust couldn't reach the household.</summary>
    public static readonly WaitingListRemovalReason NoResponse = new(2, nameof(NoResponse));

    /// <summary>What the household declared turned out to be false.</summary>
    public static readonly WaitingListRemovalReason FalseInformation = new(3, nameof(FalseInformation));

    /// <summary>Any other reason. The removal needs a note.</summary>
    public static readonly WaitingListRemovalReason Other = new(4, nameof(Other));

    private WaitingListRemovalReason(int value, string displayName) : base(value, displayName) { }
}
