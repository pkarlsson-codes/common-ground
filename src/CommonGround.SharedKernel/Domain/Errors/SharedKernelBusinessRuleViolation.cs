using CommonGround.SharedKernel.Domain.Exceptions;

namespace CommonGround.SharedKernel.Domain.Errors;

/// <summary>Factories for business rule violations raised by shared kernel value objects.</summary>
public static class BusinessRuleViolation
{
    /// <summary>Violations raised by <see cref="PhoneNumber"/>.</summary>
    public static class Phone
    {
        /// <summary>Creates the violation for a missing phone number.</summary>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException Required()
            => new("shared.phone.required");

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

    /// <summary>Violations raised by <see cref="EmailAddress"/>.</summary>
    public static class Email
    {
        /// <summary>Creates the violation for a missing email address.</summary>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException Required()
            => new("shared.email.required");

        /// <summary>Creates the violation for input that isn't shaped like an email address.</summary>
        /// <param name="email">The rejected input.</param>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException InvalidFormat(string email)
            => new(
                "shared.email.invalid_format",
                new Dictionary<string, object?>
                {
                    ["email"] = email,
                });
    }

    /// <summary>Violations raised by <see cref="Domain.PersonName"/>.</summary>
    public static class PersonName
    {
        /// <summary>Creates the violation for a missing name.</summary>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException Required()
            => new("shared.person_name.required");

        /// <summary>Creates the violation for a name longer than <see cref="Domain.PersonName.MaxLength"/>.</summary>
        /// <param name="length">The rejected name's length.</param>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException TooLong(int length)
            => new(
                "shared.person_name.too_long",
                new Dictionary<string, object?>
                {
                    ["length"] = length,
                    ["maxLength"] = Domain.PersonName.MaxLength,
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

    /// <summary>Violations raised by <see cref="Domain.Money"/>.</summary>
    public static class Money
    {
        /// <summary>Creates the violation for an amount with a fraction of a cent.</summary>
        /// <param name="amount">The rejected amount.</param>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException FractionOfCent(decimal amount)
            => new(
                "shared.money.fraction_of_cent",
                new Dictionary<string, object?>
                {
                    ["amount"] = amount,
                });
    }

    /// <summary>Violations raised by <see cref="Domain.IncomeLimitLevel"/>.</summary>
    public static class IncomeLimitLevel
    {
        /// <summary>Creates the violation for a percent of AMI outside the allowed range.</summary>
        /// <param name="percentOfAmi">The rejected percent.</param>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException PercentOutOfRange(int percentOfAmi)
            => new(
                "shared.income_limit_level.percent_out_of_range",
                new Dictionary<string, object?>
                {
                    ["percentOfAmi"] = percentOfAmi,
                    ["minimum"] = Domain.IncomeLimitLevel.MinimumPercentOfAmi,
                    ["maximum"] = Domain.IncomeLimitLevel.MaximumPercentOfAmi,
                });

        /// <summary>Creates the violation for a level without one limit per printed household size.</summary>
        /// <param name="percentOfAmi">The level's percent.</param>
        /// <param name="count">The number of limits given.</param>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException WrongNumberOfLimits(int percentOfAmi, int count)
            => new(
                "shared.income_limit_level.wrong_number_of_limits",
                new Dictionary<string, object?>
                {
                    ["percentOfAmi"] = percentOfAmi,
                    ["count"] = count,
                    ["requiredCount"] = Domain.IncomeLimitLevel.PrintedHouseholdSizes,
                });

        /// <summary>Creates the violation for a limit of zero or less.</summary>
        /// <param name="percentOfAmi">The level's percent.</param>
        /// <param name="householdSize">The household size whose limit was rejected.</param>
        /// <returns>The business rule violation exception.</returns>
        public static BusinessRuleViolationException LimitNotPositive(int percentOfAmi, int householdSize)
            => new(
                "shared.income_limit_level.limit_not_positive",
                new Dictionary<string, object?>
                {
                    ["percentOfAmi"] = percentOfAmi,
                    ["householdSize"] = householdSize,
                });
    }
}
