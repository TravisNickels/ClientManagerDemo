using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Events;

public class ClientCreated : IEvent
{
    public Guid ClientId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
}
