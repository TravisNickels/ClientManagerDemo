using ClientManager.Shared.DTOs.Responses;
using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Events;

public class ClientDeleted : IEvent, IEventToResponse<ClientResponse>
{
    public Guid Id { get; set; }

    public ClientResponse ToResponse() => new(Id, string.Empty, string.Empty, string.Empty, false, []);
}
