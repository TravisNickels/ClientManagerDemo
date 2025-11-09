namespace ClientManager.Shared.Messaging;

public class MessageContextAccessor : IMessageContextAccessor
{
    public MessageContext? Current { get; set; }
}
