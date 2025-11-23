using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Contracts.Events;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using ClientManager.Worker.Repositories;

namespace ClientManager.Worker.Handlers;

public class UpdateClientHandler(IClientRepository clientRepository, IMessagePublisher messagePublisher, ILogger<CreateClientHandler> logger) : IHandleMessage<UpdateClient>
{
    readonly IClientRepository _clientRepository = clientRepository;
    readonly ILogger<CreateClientHandler> _logger = logger;
    readonly IMessagePublisher _messagePublisher = messagePublisher;

    public async Task HandleAsync(UpdateClient message, MessageContext context, CancellationToken cancellationToken)
    {
        await _clientRepository.UpdateAsync(
            new Client
            {
                Id = message.Id,
                FirstName = message.FirstName,
                LastName = message.LastName,
                Email = message.Email,
                IsArchived = message.IsArchived
            },
            cancellationToken
        );

        _logger.LogInformation("Client {id} updated", message.Id);

        var updatedClientEvent = new ClientUpdated
        {
            Id = message.Id,
            FirstName = message.FirstName,
            LastName = message.LastName,
            Email = message.Email,
            IsArchived = message.IsArchived
        };

        await _messagePublisher.PublishAsync(updatedClientEvent, cancellationToken);
    }
}
