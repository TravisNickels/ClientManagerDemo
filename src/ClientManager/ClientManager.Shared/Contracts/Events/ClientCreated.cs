using ClientManager.Shared.DTOs.Responses;
using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Events;

public class ClientCreated : IEvent, IEventToResponse<ClientResponse>
{
    public Guid ClientId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;

    public ClientResponse ToResponse() => new(ClientId, FirstName, LastName, Email);
}
