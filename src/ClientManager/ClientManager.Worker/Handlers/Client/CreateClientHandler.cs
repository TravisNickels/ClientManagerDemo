using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using ClientManager.Worker.Repositories;

namespace ClientManager.Worker.Handlers;

public class CreateClientHandler(IClientRepository clientRepository, ILogger<CreateClientHandler> logger) : IMessageHandler<CreateClient>
{
    readonly IClientRepository _clientRepository = clientRepository;
    readonly ILogger<CreateClientHandler> _logger = logger;

    public async Task HandleAsync(CreateClient message, CancellationToken cancellationToken)
    {
        await _clientRepository.AddAsync(
            new Client
            {
                Id = message.Id,
                FirstName = message.FirstName,
                LastName = message.LastName,
                Email = message.Email,
            }
        );

        _logger.LogInformation("ClientCreatedHandler processed {client}", message.FirstName);
        //await notifier.EmitEventAsync("client.created", message);
    }
}
