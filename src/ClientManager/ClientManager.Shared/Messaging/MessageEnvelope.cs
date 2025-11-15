using System.Text.Json;

namespace ClientManager.Shared.Messaging;

public record MessageEnvelope
{
    public Guid EnvelopeId { get; set; } = Guid.NewGuid();
    public string MessageType { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public Guid CausationId { get; init; } = Guid.Empty;
    public DateTimeOffset CreatedUtc { get; } = DateTimeOffset.UtcNow;
    public JsonElement Payload { get; init; }
}
