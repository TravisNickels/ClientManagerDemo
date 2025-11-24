using ClientManager.Shared.DTOs.Responses;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;

namespace ClientManager.Shared.Contracts.Events;

public class ClientArchiveStatusChanged : IEvent, IEventToResponse<ClientResponse>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public bool IsArchived { get; set; }

    public List<PhoneResponse> Phones { get; set; } = default!;

    public ClientResponse ToResponse() => new(Id, FirstName, LastName, Email, IsArchived, Phones);
}
