using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.Identity.Application.Errors;

/// <summary>Business rule violations raised by the Identity bounded context.</summary>
public static class BusinessRuleViolation
{
    /// <summary>Rules about users.</summary>
    public static class User
    {
        /// <summary>Rules about removing a user from a trust.</summary>
        public static class RemoveFromTrust
        {
            /// <summary>The user doesn't belong to the trust.</summary>
            public static BusinessRuleViolationException NotAMember()
                => new("identity.user.remove_from_trust.not_a_member");

            /// <summary>A member tried to remove themselves.</summary>
            public static BusinessRuleViolationException CannotRemoveSelf()
                => new("identity.user.remove_from_trust.cannot_remove_self");
        }
    }
}
