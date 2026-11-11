namespace CommonGround.API.Infrastructure.Messaging;

/// <summary>Where Wolverine sends integration events, read from the <c>Messaging</c> config section.</summary>
internal sealed class MessagingOptions
{
    /// <summary>The config section name.</summary>
    public const string SectionName = "Messaging";

    /// <summary>Gets or sets the external transport. <see cref="MessageTransport.None"/> keeps every message on durable local queues.</summary>
    public MessageTransport Transport { get; set; } = MessageTransport.None;

    /// <summary>Gets or sets the PostgreSQL queue that integration events go to when <see cref="Transport"/> is <see cref="MessageTransport.Postgres"/>.</summary>
    public string PostgresQueue { get; set; } = "integration_events";

    /// <summary>Gets or sets the RabbitMQ connection used when <see cref="Transport"/> is <see cref="MessageTransport.RabbitMq"/>.</summary>
    public RabbitMqOptions RabbitMq { get; set; } = new();
}

/// <summary>The external transport for integration events.</summary>
internal enum MessageTransport
{
    /// <summary>No external transport. Handlers in this process still receive every event.</summary>
    None,

    /// <summary>Wolverine's PostgreSQL queue transport, in the API's own database.</summary>
    Postgres,

    /// <summary>RabbitMQ, one exchange per event type.</summary>
    RabbitMq,
}
