namespace ClientManager.Shared.Messaging;

public interface IMessageConsumeMiddleware
{
    Task InvokeAsync(MessageConsumeContext context, MessageConsumeDelegate finalHandler, CancellationToken cancellationToken = default);
}

public delegate Task MessageConsumeDelegate(MessageConsumeContext context, CancellationToken cancellationToken = default);
