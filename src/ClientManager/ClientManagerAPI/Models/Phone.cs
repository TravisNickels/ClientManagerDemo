using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientManagerAPI.Models;

public class Phone
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [ForeignKey("Client")]
    public Guid ClientId { get; set; }
    [Required]
    public string Number { get; set; } = string.Empty;
    [Required]
    public string Type { get; set; } = string.Empty;

    public Client Client { get; set; } = null!;
}

