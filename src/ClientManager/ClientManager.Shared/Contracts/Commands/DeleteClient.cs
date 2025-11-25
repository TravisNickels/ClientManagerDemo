using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Commands;

public class DeleteClient : ICommand
{
    public Guid Id { get; set; }
}
