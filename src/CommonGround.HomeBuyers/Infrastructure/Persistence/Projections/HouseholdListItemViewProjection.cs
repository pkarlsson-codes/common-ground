using CommonGround.HomeBuyers.Application.ReadModels;
using CommonGround.HomeBuyers.Domain.Households.Events;
using Marten.Events.Aggregation;

namespace CommonGround.HomeBuyers.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="HouseholdListItemView"/> from Household events.</summary>
internal sealed class HouseholdListItemViewProjection : SingleStreamProjection<HouseholdListItemView, string>
{
    public static HouseholdListItemView Create(HouseholdRegistered e)
    {
        var primary = e.Members.Single(m => m.IsPrimary);

        return new HouseholdListItemView()
        {
            Id = e.AggregateId,
            PrimaryMemberId = primary.MemberId,
            PrimaryMemberName = primary.Name,
            PrimaryMemberEmail = primary.Email,
            PrimaryMemberPhone = primary.Phone,
            AccessibilityRequired = e.RequiresAccessibility,
            HouseholdSize = e.Members.Count,
            RegisteredAt = e.OccurredAt,
            RegisteredBy = e.RegisteredById
        };
    }

    public static void Apply(HouseholdListItemView view, HouseholdMemberAdded _)
    {
        view.HouseholdSize++;
    }

    public static void Apply(HouseholdListItemView view, HouseholdMemberRemoved _)
    {
        view.HouseholdSize--;
    }

    public static void Apply(HouseholdListItemView view, HouseholdMemberUpdated e)
    {
        if (e.MemberId != view.PrimaryMemberId) return;

        view.PrimaryMemberName = e.Name;
        view.PrimaryMemberEmail = e.Email;
        view.PrimaryMemberPhone = e.Phone;
    }

    public static void Apply(HouseholdListItemView view, HouseholdCircumstancesUpdated e)
    {
        view.AccessibilityRequired = e.RequiresAccessibility;
    }

    public static void Apply(HouseholdListItemView view, HousingCounselingRecorded _)
    {
        view.IsHousingCounselingCompleted = true;
    }

    public static void Apply(HouseholdListItemView view, CltOrientationRecorded _)
    {
        view.IsCltOrientationCompleted = true;
    }

    public static void Apply(HouseholdListItemView view, MortgagePrequalificationRecorded e)
    {
        view.MortgagePrequalificationExpiryDate = e.ExpiryDate;
    }

    public static void Apply(HouseholdListItemView view, MortgagePrequalificationExpired _)
    {
        view.MortgagePrequalificationExpiryDate = null;
    }
}
