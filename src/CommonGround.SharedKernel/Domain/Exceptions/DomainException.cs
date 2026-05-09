namespace CommonGround.SharedKernel.Domain.Exceptions;

/// <summary>
/// Base exception class for domain-specific exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DomainException class.
    /// </summary>
    protected DomainException()
    {
        Args = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    protected DomainException(string message)
        : base(message)
    {
        Args = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    protected DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        Args = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class with a stable error code,
    /// message template context arguments, and an optional inner exception.
    /// </summary>
    /// <param name="code">The stable machine-readable error code.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="args">Named metadata arguments for message template rendering.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    protected DomainException(
        string code,
        string message,
        IReadOnlyDictionary<string, object?>? args = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = string.IsNullOrWhiteSpace(code)
            ? throw new ArgumentException("Error code must be provided.", nameof(code))
            : code;

        Args = args ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class with a stable error code,
    /// message template context arguments, and an optional inner exception.
    /// </summary>
    /// <param name="code">The stable machine-readable error code.</param>
    /// <param name="args">Named metadata arguments for message template rendering.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    protected DomainException(
        string code,
        IReadOnlyDictionary<string, object?>? args = null,
        Exception? innerException = null)
        : base(code, innerException)
    {
        Code = string.IsNullOrWhiteSpace(code)
            ? throw new ArgumentException("Error code must be provided.", nameof(code))
            : code;

        Args = args ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Gets the stable machine-readable error code associated with the exception.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Gets named metadata arguments used to render user-facing error messages.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Args { get; }
}