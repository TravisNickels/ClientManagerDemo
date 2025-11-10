using System.ComponentModel.DataAnnotations;
using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Commands;

public class CreateClient : ICommand
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Client must have a first name.")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Client must have a last name.")]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "Client must have an email.")]
    [EmailAddress(ErrorMessage = "Client must have a valid email address.")]
    public string Email { get; set; } = default!;
}
