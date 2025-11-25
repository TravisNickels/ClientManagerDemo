using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Contracts.Events;
using ClientManager.Shared.Messaging;
using ClientManager.Worker.Repositories;

namespace ClientManager.Worker.Handlers;

public class DeleteClientHandler(IClientRepository clientRepository, IMessagePublisher messagePublisher, ILogger<CreateClientHandler> logger) : IHandleMessage<DeleteClient>
{
    readonly IClientRepository _clientRepository = clientRepository;
    readonly ILogger<CreateClientHandler> _logger = logger;
    readonly IMessagePublisher _messagePublisher = messagePublisher;

    public async Task HandleAsync(DeleteClient message, MessageContext context, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.DeleteAsync(message.Id, cancellationToken);

        _logger.LogInformation("Client {id} deleted", message.Id);

        var deletedEvent = new ClientDeleted
        {
            Id = message.Id,
            FirstName = client.FirstName,
            LastName = client.LastName
        };

        await _messagePublisher.PublishAsync(deletedEvent, cancellationToken);
    }
}
