using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Domain.Errors;

/// <summary>Factories for business rule violations raised by shared kernel value objects.</summary>
public static class BusinessRuleViolation
{
    /// <summary>Violations raised by <see cref="PhoneNumber"/>.</summary>
    public static class Phone
    {
        /// <summary>Creates the violation for input that cannot be parsed as a valid phone number.</summary>
        /// <param name="phone">The rejected input.</param>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException InvalidFormat(string phone)
            => new(
                "shared.phone.invalid_format",
                new Dictionary<string, object?>
                {
                    ["phone"] = phone,
                });
    }
}
