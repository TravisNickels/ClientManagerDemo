namespace ClientManager.Shared.Messaging;

public interface IMessageContextAccessor
{
    MessageContext? Current { get; }
    MessageContext GetOrCreateContext();
    void SetCurrentContext(MessageContext context);
    void SetCausationId(Guid messageId);

    void ClearContext();
}
