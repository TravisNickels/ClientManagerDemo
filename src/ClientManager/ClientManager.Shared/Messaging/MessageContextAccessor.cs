namespace ClientManager.Shared.Messaging;

public class MessageContextAccessor : IMessageContextAccessor
{
    static readonly AsyncLocal<MessageContext?> _current = new();

    public MessageContext GetOrCreateContext()
    {
        Current ??= new MessageContext(Guid.NewGuid(), null, DateTimeOffset.UtcNow);
        return Current;
    }

    public void SetCurrentContext(MessageContext context)
    {
        Current = context;
    }

    public void SetCausationId(Guid messageId)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentNullException(nameof(messageId), "The provided causationId is an Empty Guid");

        Current ??= GetOrCreateContext();

        Current = new MessageContext(Current.CorrelationId, messageId, Current.Timestamp);
    }

    public MessageContext? Current
    {
        get => _current.Value;
        private set => _current.Value = value;
    }
}
