using CommonGround.SharedKernel.Domain;

namespace CommonGround.HomeBuyers.Domain.Households;

/// <summary>Mortgage prequalification issued by a lender.</summary>
public sealed class MortgagePrequalification : ValueObject
{
    public MortgagePrequalification(string lenderName, decimal approvedAmount, DateOnly expiryDate)
    {
        LenderName = lenderName;
        ApprovedAmount = approvedAmount;
        ExpiryDate = expiryDate;
    }
    
    public string LenderName { get; init; }
    public decimal ApprovedAmount { get; init; }
    public DateOnly ExpiryDate { get; init; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return LenderName;
        yield return ApprovedAmount;
        yield return ExpiryDate;
    }
}