namespace CommonGround.API.Infrastructure;

/// <summary>Configuration options for RabbitMQ connection.</summary>
internal sealed class RabbitMqOptions
{
    /// <summary>Gets or sets the hostname.</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>Gets or sets the port.</summary>
    public int Port { get; set; } = 5672;

    /// <summary>Gets or sets the username.</summary>
    public string Username { get; set; } = "dev_user";

    /// <summary>Gets or sets the password.</summary>
    public string Password { get; set; } = "dev_password";
}