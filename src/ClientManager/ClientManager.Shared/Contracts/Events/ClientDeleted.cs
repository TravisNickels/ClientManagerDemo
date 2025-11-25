using ClientManager.Shared.DTOs.Responses;
using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Events;

public class ClientDeleted : IEvent, IEventToResponse<ClientResponse>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public ClientResponse ToResponse() => new(Id, FirstName, LastName, string.Empty, false, []);
}
