using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Commands;

public class ChangeClientArchiveStatus : ICommand
{
    public Guid Id { get; set; }
    public bool IsArchived { get; set; }
}
