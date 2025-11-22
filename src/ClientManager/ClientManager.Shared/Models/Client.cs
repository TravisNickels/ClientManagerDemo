using System.ComponentModel.DataAnnotations;

namespace ClientManager.Shared.Models;

public class Client
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public bool IsArchived { get; set; } = false;

    public List<Phone> Phones { get; set; } = [];
}
