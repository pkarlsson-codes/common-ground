using CommonGround.HomeBuyers.Application.Errors;
using CommonGround.HomeBuyers.Domain.HomeownerApplications;
using CommonGround.HomeBuyers.Domain.HomeownerApplications.Events;
using CommonGround.HomeBuyers.Domain.Households.Events;
using CommonGround.SharedKernel.Domain;
using CommonGround.SharedKernel.Domain.Events;

namespace CommonGround.HomeBuyers.Domain.Households;

public sealed class Household : AggregateRoot<HouseholdId>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Household() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private readonly List<HouseholdMember> _members = [];
    public IReadOnlyList<HouseholdMember> Members { get => _members.AsReadOnly(); }
    public HouseholdMember? PrimaryMember => Members.FirstOrDefault(m => m.IsPrimary);
    public HouseholdCircumstances Circumstances { get; private set; }
    public MortgagePrequalification? MortgagePrequalification { get; private set; }
    public bool IsHousingCounselingCompleted { get; private set; }
    public bool IsCltOrientationCompleted { get; private set; }
    public Declaration Declare() => new(Members.Count, Circumstances);

    /// <summary>Readiness items the household has not completed yet; empty when readiness is complete.</summary>
    public IReadOnlyList<string> OutstandingReadinessItems
    {
        get
        {
            var items = new List<string>();
            if (!IsHousingCounselingCompleted) items.Add("HousingCounseling");
            if (!IsCltOrientationCompleted) items.Add("CltOrientation");
            if (MortgagePrequalification is null) items.Add("MortgagePrequalification");
            return items;
        }
    }

    /// <summary>Whether counseling, orientation and a mortgage prequalification are all on record.</summary>
    public bool IsReadinessComplete => OutstandingReadinessItems.Count == 0;

    /// <summary>Whether the household holds a mortgage prequalification that is still valid on <paramref name="date"/>.</summary>
    public bool IsPrequalificationValidOn(DateOnly date) =>
        MortgagePrequalification is not null && MortgagePrequalification.ExpiryDate >= date;

    /// <summary>
    /// Registers a household.
    /// </summary>
    /// <returns>The registered household.</returns>
    public static HouseholdRegistered Register(HouseholdId id, List<HouseholdMember> members, HouseholdCircumstances circumstances, UserId registeredById)
    {
        ArgumentNullException.ThrowIfNull(members);
        if (members.Count(m => m.IsPrimary) != 1)
            throw BusinessRuleViolation.Household.Register.MustHaveExactlyOnePrimaryMember();

        return new HouseholdRegistered(
                id.Value,
                [..members.Select(m => new RegisteredMember(m.Id.Value, m.Name, m.Email, m.Phone, m.IsPrimary))],
                circumstances.AnnualIncome,
                circumstances.RequiresAccessibility,
                registeredById.Value);
    }
    
    /// <summary>Adds a non-primary member to the household.</summary>
    public HouseholdMemberAdded AddMember(HouseholdMemberId memberId, string name, string email, string phone, UserId addedBy)
    {
        var member = HouseholdMember.Create(memberId, name, email, phone, isPrimary: false);

        return new HouseholdMemberAdded(
            Id.Value, member.Id.Value, member.Name, member.Email, member.Phone, addedBy.Value);
    }

    /// <summary>Updates a member's contact details. No-op when nothing changed.</summary>
    public HouseholdMemberUpdated? UpdateMember(HouseholdMemberId memberId, string name, string email, string phone, UserId updatedBy)
    {
        HouseholdMember.ValidateContactDetails(name, email, phone);
        var member = FindMember(memberId);
        if (member.Name == name && member.Email == email && member.Phone == phone) return null;

        return new HouseholdMemberUpdated(Id.Value, memberId.Value, name, email, phone, updatedBy.Value);
    }

    /// <summary>Removes a non-primary member from the household.</summary>
    public HouseholdMemberRemoved RemoveMember(HouseholdMemberId memberId, UserId removedBy)
    {
        var member = FindMember(memberId);
        if (member.IsPrimary)
            throw BusinessRuleViolation.Household.Members.CannotRemovePrimaryMember(memberId);

        return new HouseholdMemberRemoved(Id.Value, memberId.Value, removedBy.Value);
    }

    /// <summary>Replaces the household's circumstances. No-op when nothing changed.</summary>
    public HouseholdCircumstancesUpdated? UpdateCircumstances(HouseholdCircumstances circumstances, UserId updatedBy)
    {
        ArgumentNullException.ThrowIfNull(circumstances);
        if (Circumstances == circumstances) return null;

        return new HouseholdCircumstancesUpdated(
            Id.Value, circumstances.AnnualIncome, circumstances.RequiresAccessibility, updatedBy.Value);
    }

    /// <summary>Records CLT orientation completion. Idempotent.</summary>
    public CltOrientationRecorded? RecordCltOrientation(DateOnly completedDate, UserId recordedBy)
    {
        if (IsCltOrientationCompleted) return null;

        return new CltOrientationRecorded(Id.Value, completedDate, recordedBy.Value);
    }

    /// <summary>Records HUD-approved housing counseling completion. Idempotent.</summary>
    public HousingCounselingRecorded? RecordHousingCounseling(
        string certificationNumber, DateOnly completedDate,
        string counselorOrganization, UserId recordedBy)
    {
        if (IsHousingCounselingCompleted) return null;

        return new HousingCounselingRecorded(
            Id.Value, certificationNumber, completedDate, counselorOrganization, recordedBy.Value);
    }

    /// <summary>Expires prequalification.</summary>
    /// <returns>The expiry, or <see langword="null"/> when the prequalification has since been replaced or expired.</returns>
    public MortgagePrequalificationExpired? ExpirePrequalification(DateOnly originalExpiryDate)
    {
        if (MortgagePrequalification?.ExpiryDate != originalExpiryDate) return null;

        return new MortgagePrequalificationExpired(Id.Value, originalExpiryDate);
    }

    /// <summary>Records a mortgage prequalification.</summary>
    public MortgagePrequalificationRecorded RecordMortgagePrequalification(
        string lenderName, decimal approvedAmount,
        DateOnly expiryDate, UserId recordedBy)
    {
        return new MortgagePrequalificationRecorded(
            Id.Value, lenderName, approvedAmount, expiryDate, recordedBy.Value);
    }
    

    /// <summary>Starts a household from its first event.</summary>
    public static Household Create(HouseholdRegistered e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var household = new Household();
        household.Apply(e);
        return household;
    }

    private void Apply(HouseholdRegistered e)
    {
        Id = HouseholdId.Create(e.AggregateId);
        Circumstances = HouseholdCircumstances.Create(e.RequiresAccessibility, e.AnnualIncome);
        _members.AddRange(e.Members.Select(m => HouseholdMember.Reconstitute(HouseholdMemberId.Create(m.MemberId), m.Name, m.Email, m.Phone, m.IsPrimary)));
    }
    
    /// <summary>Applies <see cref="HouseholdMemberAdded"/>.</summary>
    public void Apply(HouseholdMemberAdded e)
    {
        _members.Add(HouseholdMember.Reconstitute(HouseholdMemberId.Create(e.MemberId), e.Name, e.Email, e.Phone, isPrimary: false));
    }

    /// <summary>Applies <see cref="HouseholdMemberUpdated"/>.</summary>
    public void Apply(HouseholdMemberUpdated e)
    {
        FindMember(HouseholdMemberId.Create(e.MemberId)).ChangeContactDetails(e.Name, e.Email, e.Phone);
    }

    /// <summary>Applies <see cref="HouseholdMemberRemoved"/>.</summary>
    public void Apply(HouseholdMemberRemoved e)
    {
        _members.RemoveAll(m => m.Id.Value == e.MemberId);
    }

    /// <summary>Applies <see cref="HouseholdCircumstancesUpdated"/>.</summary>
    public void Apply(HouseholdCircumstancesUpdated e)
    {
        Circumstances = HouseholdCircumstances.Create(e.RequiresAccessibility, e.AnnualIncome);
    }

    /// <summary>Applies <see cref="MortgagePrequalificationRecorded"/>.</summary>
    public void Apply(MortgagePrequalificationRecorded e)
    {
        MortgagePrequalification = new MortgagePrequalification(
            e.LenderName, e.ApprovedAmount, e.ExpiryDate);
    }

    /// <summary>Applies <see cref="MortgagePrequalificationExpired"/>.</summary>
    public void Apply(MortgagePrequalificationExpired _)
    {
        MortgagePrequalification = null;
    }

    /// <summary>Applies <see cref="HousingCounselingRecorded"/>.</summary>
    public void Apply(HousingCounselingRecorded _)
    {
        IsHousingCounselingCompleted = true;
    }

    /// <summary>Applies <see cref="CltOrientationRecorded"/>.</summary>
    public void Apply(CltOrientationRecorded _)
    {
        IsCltOrientationCompleted = true;
    }

    private HouseholdMember FindMember(HouseholdMemberId memberId)
    {
        return _members.FirstOrDefault(m => m.Id == memberId)
            ?? throw BusinessRuleViolation.Household.Members.MemberNotFound(memberId);
    }
}