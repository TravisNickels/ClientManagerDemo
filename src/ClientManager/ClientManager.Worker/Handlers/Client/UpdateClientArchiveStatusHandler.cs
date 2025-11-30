using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Contracts.Events;
using ClientManager.Shared.Messaging;
using ClientManager.Worker.Repositories;

namespace ClientManager.Worker.Handlers;

public class UpdateClientArchiveStatusHandler(IClientRepository clientRepository, IMessagePublisher messagePublisher, ILogger<CreateClientHandler> logger)
    : IHandleMessage<UpdateClientArchiveStatus>
{
    readonly IClientRepository _clientRepository = clientRepository;
    readonly ILogger<CreateClientHandler> _logger = logger;
    readonly IMessagePublisher _messagePublisher = messagePublisher;

    public async Task HandleAsync(UpdateClientArchiveStatus message, MessageContext context, CancellationToken cancellationToken)
    {
        await _clientRepository.UpdateArchiveStatus(message.Id, message.IsArchived, cancellationToken);

        var client = await _clientRepository.GetByIdAsync(message.Id);

        _logger.LogInformation("Client with ID {ClientId} archive status changed to {IsArchived}", message.Id, message.IsArchived);

        var archiveStatusChangedEvent = new ClientArchiveStatusChanged
        {
            Id = message.Id,
            FirstName = client?.FirstName ?? "",
            LastName = client?.LastName ?? "",
            IsArchived = message.IsArchived
        };

        await _messagePublisher.PublishAsync(archiveStatusChangedEvent, cancellationToken);
    }
}
