using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Commands;

public class UpdateClientArchiveStatus : ICommand
{
    public Guid Id { get; set; }
    public bool IsArchived { get; set; }
}
