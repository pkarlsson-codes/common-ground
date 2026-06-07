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
}
