using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Commands;

public class CreateClient : ICommand
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
}
