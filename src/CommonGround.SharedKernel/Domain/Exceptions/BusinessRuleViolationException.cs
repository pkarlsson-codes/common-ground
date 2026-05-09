namespace CommonGround.SharedKernel.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated during domain operations.
/// </summary>
public sealed class BusinessRuleViolationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the BusinessRuleViolationException class with a stable error code.
    /// </summary>
    /// <param name="code">The stable machine-readable business rule code.</param>
    public BusinessRuleViolationException(string code)
        : base(code)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BusinessRuleViolationException class with a stable error code
    /// and message template context arguments.
    /// </summary>
    /// <param name="code">The stable machine-readable business rule code.</param>
    /// <param name="args">Named metadata arguments for message template rendering.</param>
    public BusinessRuleViolationException(
        string code,
        IReadOnlyDictionary<string, object?>? args)
        : base(code, args)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BusinessRuleViolationException class with a stable error code and message.
    /// </summary>
    /// <param name="code">The stable machine-readable business rule code.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="args">Named metadata arguments for message template rendering.</param>
    public BusinessRuleViolationException(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? args = null)
        : base(code, message, args)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BusinessRuleViolationException class with a stable error code,
    /// message template context arguments, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="code">The stable machine-readable business rule code.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="args">Named metadata arguments for message template rendering.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BusinessRuleViolationException(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? args,
        Exception innerException)
        : base(code, message, args, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BusinessRuleViolationException class with a stable error code,
    /// message template context arguments, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="code">The stable machine-readable business rule code.</param>
    /// <param name="args">Named metadata arguments for message template rendering.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public BusinessRuleViolationException(
        string code,
        IReadOnlyDictionary<string, object?>? args,
        Exception innerException)
        : base(code, args, innerException)
    {
    }
}
