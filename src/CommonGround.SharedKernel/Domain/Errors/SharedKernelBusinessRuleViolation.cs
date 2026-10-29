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

    /// <summary>Violations raised by <see cref="IanaTimeZone"/>.</summary>
    public static class TimeZone
    {
        /// <summary>Creates the violation for a missing time zone.</summary>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException Required()
            => new("shared.time_zone.required");

        /// <summary>Creates the violation for a value that is not a known IANA time zone id.</summary>
        /// <param name="timeZone">The rejected input.</param>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException Unknown(string timeZone)
            => new(
                "shared.time_zone.unknown",
                new Dictionary<string, object?>
                {
                    ["timeZone"] = timeZone,
                });
    }
}
