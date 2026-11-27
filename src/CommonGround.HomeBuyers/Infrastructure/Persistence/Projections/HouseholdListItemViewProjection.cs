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
            Members = [.. e.Members.Select(m => new HouseholdMemberEntry { Id = m.MemberId, Name = m.Name, Email = m.Email })],
            AccessibilityRequired = e.RequiresAccessibility,
            HouseholdSize = e.Members.Count,
            RegisteredAt = e.OccurredAt,
            RegisteredBy = e.RegisteredById
        };
    }

    public static void Apply(HouseholdListItemView view, HouseholdMemberAdded e)
    {
        view.Members.Add(new HouseholdMemberEntry { Id = e.MemberId, Name = e.Name, Email = e.Email });
        view.HouseholdSize++;
    }

    public static void Apply(HouseholdListItemView view, HouseholdMemberRemoved e)
    {
        view.Members.RemoveAll(m => m.Id == e.MemberId);
        view.HouseholdSize--;
    }

    public static void Apply(HouseholdListItemView view, HouseholdMemberUpdated e)
    {
        var member = view.Members.Single(m => m.Id == e.MemberId);
        member.Name = e.Name;
        member.Email = e.Email;

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
        view.MortgagePrequalificationLapsedOn = null;
    }

    // The expiry date stays: readers judge it against their own date, so a choice dated before the expiry sees the
    // prequalification as valid whenever the scheduled expiry happened to run.
    public static void Apply(HouseholdListItemView view, MortgagePrequalificationExpired e) =>
        view.MortgagePrequalificationLapsedOn = e.OriginalExpiryDate;
}
