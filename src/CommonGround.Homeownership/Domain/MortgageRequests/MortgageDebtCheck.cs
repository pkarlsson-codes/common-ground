using CommonGround.Homeownership.Domain.GroundLeases.Mortgages;
using CommonGround.SharedKernel.Domain;

namespace CommonGround.Homeownership.Domain.MortgageRequests;

/// <summary>
/// Compares the total mortgage debt a new loan leaves on the home with the lease's debt cap (Model Lease Art 8.3):
/// the cap is the cap percent of the purchase option price the loan's appraisal gives.
/// </summary>
public sealed class MortgageDebtCheck : ValueObject
{
    private MortgageDebtCheck(Money purchaseOptionPrice, MortgageDebtCap cap, Money capAmount, Money totalDebt, decimal debtPercent)
    {
        PurchaseOptionPrice = purchaseOptionPrice;
        Cap = cap;
        CapAmount = capAmount;
        TotalDebt = totalDebt;
        DebtPercent = debtPercent;
    }

    /// <summary>Works out the check. The cap amount is rounded to the cent and the debt percent to one decimal place.</summary>
    /// <param name="purchaseOptionPrice">What the lease's formula gives from the loan's appraisal; more than zero.</param>
    /// <param name="cap">The lease's debt cap.</param>
    /// <param name="totalDebt">All mortgage debt on the home after the loan closes.</param>
    public static MortgageDebtCheck Calculate(Money purchaseOptionPrice, MortgageDebtCap cap, Money totalDebt)
    {
        ArgumentNullException.ThrowIfNull(purchaseOptionPrice);
        ArgumentNullException.ThrowIfNull(cap);
        ArgumentNullException.ThrowIfNull(totalDebt);

        if (purchaseOptionPrice <= Money.Zero)
            throw new ArgumentOutOfRangeException(nameof(purchaseOptionPrice), purchaseOptionPrice, "The purchase option price must be more than zero.");

        var capAmount = purchaseOptionPrice.Share(cap.Percent / 100m);
        var debtPercent = Math.Round(totalDebt / purchaseOptionPrice * 100m, 1, MidpointRounding.AwayFromZero);

        return new MortgageDebtCheck(purchaseOptionPrice, cap, capAmount, totalDebt, debtPercent);
    }

    /// <summary>Gets the purchase option price the loan's appraisal gives.</summary>
    public Money PurchaseOptionPrice { get; }

    /// <summary>Gets the lease's debt cap.</summary>
    public MortgageDebtCap Cap { get; }

    /// <summary>Gets the most total debt may be: the cap percent of the purchase option price.</summary>
    public Money CapAmount { get; }

    /// <summary>Gets all mortgage debt on the home after the loan closes.</summary>
    public Money TotalDebt { get; }

    /// <summary>Gets the total debt as a percent of the purchase option price, to one decimal place.</summary>
    public decimal DebtPercent { get; }

    /// <summary>Gets whether the total debt is more than the cap amount.</summary>
    public bool IsOverCap => TotalDebt > CapAmount;

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PurchaseOptionPrice;
        yield return Cap;
        yield return TotalDebt;
    }
}
