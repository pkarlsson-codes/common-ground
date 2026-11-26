namespace CommonGround.HomeBuyers.Domain.HomeSelections;

/// <summary>What choosing a household for a home settles besides who: the purchase's terms and the date.</summary>
/// <param name="GroundLeaseTemplateId">The ground lease template the purchase will use.</param>
/// <param name="PurchasePrice">The purchase price.</param>
/// <param name="SelectedDate">The date of the choice. The waiting list is judged on it, and it can't be after today.</param>
public sealed record SelectionTerms(string GroundLeaseTemplateId, decimal PurchasePrice, DateOnly SelectedDate);
