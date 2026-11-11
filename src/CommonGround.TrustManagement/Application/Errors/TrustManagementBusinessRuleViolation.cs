using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.TrustManagement.Application.Errors;

public static class BusinessRuleViolation
{
    public static class Trust
    {
        public static class Create
        {
            public static BusinessRuleViolationException NameRequired()
                => new("trust_management.trust.create.name_required");
        }

        public static class RequestOnboarding
        {
            public static BusinessRuleViolationException CorrelationIdRequired()
                => new("trust_management.trust.request_onboarding.correlation_id_required");
        }
    }

    public static class TrustInvitation
    {
        public static BusinessRuleViolationException NotFound()
            => new("trust_management.trust_invitation.not_found");

        public static class Accept
        {
            public static BusinessRuleViolationException NoLongerValid()
                => new("trust_management.trust_invitation.accept.no_longer_valid");

            public static BusinessRuleViolationException AlreadyInTrust()
                => new("trust_management.trust_invitation.accept.already_in_trust");
        }

        public static class Cancel
        {
            public static BusinessRuleViolationException AlreadyAccepted()
                => new("trust_management.trust_invitation.cancel.already_accepted");
        }
    }
}
