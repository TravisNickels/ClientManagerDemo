using Microsoft.Extensions.DependencyInjection;

namespace ClientManager.Shared.Messaging;

public class MessageConsumeContext
{
    public string RawJson { get; set; } = string.Empty;
    public MessageEnvelope? Envelope { get; set; }
    public object? Message { get; set; }
    public Type? MessageType { get; set; }
    public AsyncServiceScope? Scope { get; set; }
    public MessageContext? MessageContext { get; set; }
}
