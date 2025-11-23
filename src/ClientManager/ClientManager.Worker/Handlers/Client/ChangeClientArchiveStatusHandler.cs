using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Contracts.Events;
using ClientManager.Shared.Messaging;
using ClientManager.Worker.Repositories;

namespace ClientManager.Worker.Handlers;

public class ChangeClientArchiveStatusHandler(IClientRepository clientRepository, IMessagePublisher messagePublisher, ILogger<CreateClientHandler> logger)
    : IHandleMessage<ChangeClientArchiveStatus>
{
    readonly IClientRepository _clientRepository = clientRepository;
    readonly ILogger<CreateClientHandler> _logger = logger;
    readonly IMessagePublisher _messagePublisher = messagePublisher;

    public async Task HandleAsync(ChangeClientArchiveStatus message, MessageContext context, CancellationToken cancellationToken)
    {
        await _clientRepository.UpdateArchiveStatus(message.Id, message.IsArchived, cancellationToken);

        _logger.LogInformation("Client with ID {ClientId} archive status changed to {IsArchived}", message.Id, message.IsArchived);

        var archiveStatusChangedEvent = new ClientArchiveStatusChanged { Id = message.Id, IsArchived = message.IsArchived };

        await _messagePublisher.PublishAsync(archiveStatusChangedEvent, cancellationToken);
    }
}
