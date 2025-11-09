namespace ClientManager.Shared.Messaging;

public class MessageEnvelope<T>
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Guid CausationId { get; set; } = Guid.Empty;
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
    public T Message { get; init; } = default!;
}
