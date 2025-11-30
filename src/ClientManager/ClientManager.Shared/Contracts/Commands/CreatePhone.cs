using System.ComponentModel.DataAnnotations;
using ClientManager.Shared.Messaging;

namespace ClientManager.Shared.Contracts.Commands;

public class CreatePhone : ICommand
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The phone requires a client Id foreign key")]
    public Guid ClientId { get; set; }

    [Required(ErrorMessage = "A phone must have a number")]
    [RegularExpression(@"^\+\d \(\d{3}\) \d{3}-\d{4}$", ErrorMessage = "Phone number must match +# (###) ###-####")]
    public string PhoneNumber { get; set; } = default!;

    [Required(ErrorMessage = "A phone must have a type")]
    public string PhoneType { get; set; } = default!;
}
