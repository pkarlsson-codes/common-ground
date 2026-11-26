using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>What a home selection decided for one application on the waiting list.</summary>
public sealed class SelectionOutcome : Enumeration<SelectionOutcome>
{
    /// <summary>The household was chosen for the home.</summary>
    public static readonly SelectionOutcome Chosen = new(1, nameof(Chosen));

    /// <summary>The household could have been chosen, and another household was chosen instead.</summary>
    public static readonly SelectionOutcome PassedOver = new(2, nameof(PassedOver));

    /// <summary>The household couldn't have been chosen on the selection's date, for the reasons recorded with it.</summary>
    public static readonly SelectionOutcome CouldNotBeChosen = new(3, nameof(CouldNotBeChosen));

    private SelectionOutcome(int value, string displayName) : base(value, displayName) { }
}
