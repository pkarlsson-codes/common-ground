using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.Households.Events;
using Marten.Events.Aggregation;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HouseholdView"/> from Household events.</summary>
internal sealed class HouseholdViewProjection : SingleStreamProjection<HouseholdView, string>
{
    public static HouseholdView Create(HouseholdRegistered e) =>
        new()
        {
            Id = e.AggregateId,
            Members = [.. e.Members.Select(m => new HouseholdMemberEntry
            {
                Id = m.MemberId,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                IsPrimary = m.IsPrimary,
            })],
            HouseholdSize = e.Members.Count,
            AnnualIncome = e.AnnualIncome,
            AccessibilityRequired = e.RequiresAccessibility,
            RegisteredAt = e.OccurredAt,
            RegisteredBy = e.RegisteredById,
        };

    public static void Apply(HouseholdView view, HouseholdMemberAdded e)
    {
        view.Members.Add(new HouseholdMemberEntry
        {
            Id = e.MemberId,
            Name = e.Name,
            Email = e.Email,
            Phone = e.Phone,
            IsPrimary = false,
        });
        view.HouseholdSize = view.Members.Count;
    }

    public static void Apply(HouseholdView view, HouseholdMemberUpdated e)
    {
        var member = view.Members.Single(m => m.Id == e.MemberId);
        member.Name = e.Name;
        member.Email = e.Email;
        member.Phone = e.Phone;
    }

    public static void Apply(HouseholdView view, HouseholdMemberRemoved e)
    {
        view.Members.RemoveAll(m => m.Id == e.MemberId);
        view.HouseholdSize = view.Members.Count;
    }

    public static void Apply(HouseholdView view, HouseholdCircumstancesUpdated e)
    {
        view.AnnualIncome = e.AnnualIncome;
        view.AccessibilityRequired = e.RequiresAccessibility;
    }

    public static void Apply(HouseholdView view, MortgagePrequalificationRecorded e)
    {
        view.MortgagePrequalification = new MortgagePrequalificationEntry
        {
            LenderName = e.LenderName,
            ApprovedAmount = e.ApprovedAmount,
            ExpiryDate = e.ExpiryDate,
            RecordedAt = e.OccurredAt,
        };
    }

    public static void Apply(HouseholdView view, MortgagePrequalificationExpired _)
    {
        view.MortgagePrequalification = null;
    }
    
    public static void Apply(HouseholdView view, CltOrientationRecorded e)
    {
        view.IsCltOrientationCompleted = true;
        view.CltOrientationCompletedDate = e.CompletedDate;
    }

    public static void Apply(HouseholdView view, HousingCounselingRecorded e)
    {
        view.IsHousingCounselingCompleted = true;
        view.HousingCounselingCertificationNumber = e.CertificationNumber;
        view.HousingCounselingCompletedDate = e.CompletedDate;
        view.HousingCounselorOrganization = e.CounselorOrganization;
    }
}