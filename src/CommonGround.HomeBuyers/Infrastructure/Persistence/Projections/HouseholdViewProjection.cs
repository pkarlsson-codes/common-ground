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

    public static void Apply(HouseholdView view, MortgagePrequalificationRecorded e)
    {
        view.MortgagePrequalification = new MortgagePrequalificationEntry
        {
            LenderName = e.LenderName,
            ApprovedAmount = e.ApprovedAmount,
            ExpiryDate = e.ExpiryDate,
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