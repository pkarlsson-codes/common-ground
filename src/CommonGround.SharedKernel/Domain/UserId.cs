namespace CommonGround.SharedKernel.Domain;

/// <summary>
/// Unique identifier for a User.
/// </summary>
public sealed class UserId : EntityId<UserId>
{
    private const string Prefix = "user";

    /// <summary>
    /// Initializes a new instance of the UserId class.
    /// </summary>
    /// <param name="value">The unique identifier value.</param>
    public UserId(string value)
        : base(value, Prefix)
    {
    }
}
