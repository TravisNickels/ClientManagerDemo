using System.ComponentModel.DataAnnotations;
using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Commands;

public class UpdateClient : ICommand
{
    [Required(ErrorMessage = "Client ID required to update client.")]
    public Guid Id { get; set; }

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    [EmailAddress(ErrorMessage = "Client must have a valid email address.")]
    public string Email { get; set; } = default!;

    public bool IsArchived { get; set; } = default!;
}
