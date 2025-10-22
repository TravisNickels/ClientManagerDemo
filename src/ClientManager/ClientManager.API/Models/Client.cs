using System.ComponentModel.DataAnnotations;

namespace ClientManager.API.Models;

public class Client
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [StringLength(100)]
    public string? FirstName { get; set; }
    [Required]
    [StringLength(100)]
    public string? LastName { get; set; }
    public string? Email { get; set; }

    public List<Phone> Phones { get; set; } = [];
}

