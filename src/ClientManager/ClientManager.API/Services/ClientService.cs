using System.ComponentModel.DataAnnotations;
using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Data;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using RabbitMQ.Client.Exceptions;

namespace ClientManager.API.Services;

public class ClientService(IMessagePublisher publisher, ReadOnlyAppDbContext readOnlyAppDbContext) : IClientService
{
    readonly IMessagePublisher _messagePublisher = publisher;

    readonly ReadOnlyAppDbContext _readOnlyAppDbContext = readOnlyAppDbContext;

    public async Task<CreateClient> SendCreateClientMessage(CreateClient message)
    {
        ValidateClient(message);

        try
        {
            await _messagePublisher.PublishAsync(message);
            return message;
        }
        catch (PublishException)
        {
            throw;
        }
    }

    public Task<Client> GetClientById(Guid id)
    {
        throw new NotImplementedException();
    }

    static void ValidateClient(CreateClient message)
    {
        if (string.IsNullOrWhiteSpace(message.FirstName))
            throw new ArgumentException("Client must have a first name.", nameof(message));
        if (string.IsNullOrWhiteSpace(message.LastName))
            throw new ArgumentException("Client must have a last name.", nameof(message));
        if (string.IsNullOrWhiteSpace(message.Email))
            throw new ArgumentException("Client must have an email.", nameof(message));

        var emailAttribute = new EmailAddressAttribute();
        if (!emailAttribute.IsValid(message.Email))
            throw new ArgumentException("Client must have a valid email address.", nameof(message));

        if (message.Id == Guid.Empty)
            message.Id = Guid.NewGuid();
    }
}
