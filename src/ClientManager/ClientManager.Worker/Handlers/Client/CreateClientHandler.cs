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
                IsArchived = false,
                Phones =
                    message
                        .Phones?.Select(phone => new Phone
                        {
                            Id = phone.Id == Guid.Empty ? Guid.NewGuid() : phone.Id,
                            ClientId = phone.ClientId == Guid.Empty ? message.Id : phone.Id,
                            Number = phone.PhoneNumber,
                            Type = phone.PhoneType
                        })
                        .ToList() ?? []
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

        await _messagePublisher.PublishAsync(createdEvent, cancellationToken);
    }
}
