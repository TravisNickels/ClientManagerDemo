namespace ClientManager.Shared.Messaging;

public record MessageContext(Guid CorrelationId, Guid? CausationId, DateTimeOffset Timestamp);
