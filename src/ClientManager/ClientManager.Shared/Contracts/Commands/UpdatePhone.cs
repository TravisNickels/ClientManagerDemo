using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Commands;

public class UpdatePhone : ICommand
{
    public Guid Id { get; set; }

    public Guid ClientId { get; set; }
    public string Number { get; set; } = default!;
    public string Type { get; set; } = default!;
}
