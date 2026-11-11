using CommonGround.Identity.Application.ReadModels;
using CommonGround.Identity.Domain.Users.Events;
using JasperFx.Events;
using Marten.Events.Aggregation;

namespace CommonGround.Identity.Infrastructure.Persistence.Projections;

/// <summary>Builds <see cref="UserView"/> from User domain events.</summary>
internal sealed class UserViewProjection : SingleStreamProjection<UserView, string>
{
    /// <summary>Creates the initial projection from a UserRegistered event.</summary>
    public static UserView Create(IEvent<UserRegistered> e) =>
        new()
        {
            Id = e.Data.KeycloakSubject,
            KeycloakSubject = e.Data.KeycloakSubject,
            UserId = e.Data.AggregateId,
            Email = e.Data.Email,
            DisplayName = e.Data.DisplayName,
            TrustId = null, // User starts without a trust
        };

    /// <summary>Updates the projection when user is assigned to a trust.</summary>
    public static void Apply(UserView view, UserTrustAssigned e)
    {
        view.TrustId = e.TrustId;
    }

    /// <summary>Clears the trust when the user is removed from it.</summary>
    public static void Apply(UserView view, UserRemovedFromTrust e)
    {
        view.TrustId = null;
    }

    /// <summary>Keeps the email address and name from the latest sign-in.</summary>
    public static void Apply(UserView view, UserProfileUpdated e)
    {
        view.Email = e.Email;
        view.DisplayName = e.DisplayName;
    }
}
