using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Contracts.Events;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using ClientManager.Worker.Repositories;

namespace ClientManager.Worker.Handlers;

public class CreateClientHandler(IClientRepository clientRepository, IMessagePublisher messagePublisher, ILogger<CreateClientHandler> logger) : IHandleMessage<CreateClient>
{
    readonly IClientRepository _clientRepository = clientRepository;
    readonly ILogger<CreateClientHandler> _logger = logger;
    readonly IMessagePublisher _messagePublisher = messagePublisher;

    public async Task HandleAsync(CreateClient message, MessageContext context, CancellationToken cancellationToken)
    {
        await _clientRepository.AddAsync(
            new Client
            {
                Id = message.Id,
                FirstName = message.FirstName,
                LastName = message.LastName,
                Email = message.Email,
            },
            cancellationToken
        );

        _logger.LogInformation("CreateClientHandler processed {id}", message.Id);

        var createdEvent = new ClientCreated
        {
            Id = message.Id,
            FirstName = message.FirstName,
            LastName = message.LastName,
            Email = message.Email
        };

        await _messagePublisher.PublishAsync(createdEvent);
    }
}
