namespace ClientManager.Shared.Messaging;

public interface IMessageContextAccessor
{
    MessageContext? Current { get; set; }
}
