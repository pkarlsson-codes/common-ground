using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households;

public class Household : AggregateRoot<HouseholdId>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Household() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private bool AllChecklistItemsSatisfied =>
        IsHousingCounselingCompleted &&
        IsCltOrientationCompleted &&
        MortgagePrequalification is not null;

    private readonly List<HouseholdMember> _members = [];
    public IReadOnlyList<HouseholdMember> Members { get => _members.AsReadOnly(); }
    public HouseholdMember? PrimaryMember => Members.FirstOrDefault(m => m.IsPrimary);
    public HouseholdCircumstances Circumstances { get; private set; }
    public MortgagePrequalification? MortgagePrequalification { get; private set; }
    public bool IsHousingCounselingCompleted { get; private set; }
    public bool IsCltOrientationCompleted { get; private set; }
    public Declaration Declare() => new(Members.Count, Circumstances);

    public bool IsPrequalificationExpired =>
        MortgagePrequalification is not null &&
        MortgagePrequalification.ExpiryDate < DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Registers a household.
    /// </summary>
    /// <returns>The registered household.</returns>
    public static Household Register(List<HouseholdMember> members, HouseholdCircumstances circumstances, UserId registeredById)
    {
        ArgumentNullException.ThrowIfNull(members);
        if (members.Count(m => m.IsPrimary) != 1)
            throw BusinessRuleViolation.Household.Register.MustHaveExactlyOnePrimaryMember();

        var household = new Household();
        household.ApplyChange(
            new HouseholdRegistered(
                HouseholdId.Generate().Value,
                [..members.Select(m => new RegisteredMember(m.Id.Value, m.Name, m.Email, m.Phone, m.IsPrimary))],
                circumstances.AnnualIncome,
                circumstances.RequiresAccessibility,
                registeredById.Value));
        return household;
    }
    
    /// <summary>Records CLT orientation completion. Idempotent.</summary>
    public void RecordCltOrientation(DateOnly completedDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        if (IsCltOrientationCompleted) return;

        ApplyChange(new CltOrientationRecorded(Id.Value, completedDate, recordedBy.Value));
    }

    /// <summary>Records HUD-approved housing counseling completion. Idempotent.</summary>
    public void RecordHousingCounseling(
        string certificationNumber, DateOnly completedDate,
        string counselorOrganization, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);
        if (IsHousingCounselingCompleted) return;

        ApplyChange(new HousingCounselingRecorded(
            Id.Value, certificationNumber, completedDate, counselorOrganization, recordedBy.Value));
    }

    /// <summary>Expires prequalification.</summary>
    public bool ExpirePrequalification(DateOnly originalExpiryDate)
    {
        if (MortgagePrequalification?.ExpiryDate != originalExpiryDate) return false;

        ApplyChange(new MortgagePrequalificationExpired(Id.Value, originalExpiryDate));
        return true;
    }

    /// <summary>Records a mortgage prequalification.</summary>
    public void RecordMortgagePrequalification(
        string lenderName, decimal approvedAmount,
        DateOnly expiryDate, UserId recordedBy)
    {
        ArgumentNullException.ThrowIfNull(recordedBy);

        ApplyChange(new MortgagePrequalificationRecorded(
            Id.Value, lenderName, approvedAmount, expiryDate, recordedBy.Value));
    }
    
    protected override void ApplyEvent(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case HouseholdRegistered e: Apply(e); break;
            case MortgagePrequalificationRecorded e: Apply(e); break;
            case MortgagePrequalificationExpired e: Apply(e); break;
            case HousingCounselingRecorded e: Apply(e); break;
            case CltOrientationRecorded e: Apply(e); break;
            default: throw new InvalidOperationException(
                $"Unknown event: {domainEvent?.GetType().Name}");
        }
    }

    private void Apply(HouseholdRegistered e)
    {
        Id = HouseholdId.Create(e.AggregateId);
        Circumstances = HouseholdCircumstances.Create(e.RequiresAccessibility, e.AnnualIncome);
        _members.AddRange(e.Members.Select(m => HouseholdMember.Reconstitute(HouseholdMemberId.Create(m.MemberId), m.Name, m.Email, m.Phone, m.IsPrimary)));
    }
    
    private void Apply(MortgagePrequalificationRecorded e)
    {
        MortgagePrequalification = new MortgagePrequalification(
            e.LenderName, e.ApprovedAmount, e.ExpiryDate);
    }

    private void Apply(MortgagePrequalificationExpired _)
    {
        MortgagePrequalification = null;
    }

    private void Apply(HousingCounselingRecorded _)
    {
        IsHousingCounselingCompleted = true;
    }

    private void Apply(CltOrientationRecorded _)
    {
        IsCltOrientationCompleted = true;
    }
}