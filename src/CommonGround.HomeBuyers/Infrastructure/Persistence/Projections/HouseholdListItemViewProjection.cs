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
            PrimaryMemberName = primary.Name,
            PrimaryMemberEmail = primary.Email,
            PrimaryMemberPhone = primary.Phone,
            AccessibilityRequired = e.RequiresAccessibility,
            HouseholdSize = e.Members.Count,
            RegisteredAt = e.OccurredAt,
            RegisteredBy = e.RegisteredById
        };
    }
}
