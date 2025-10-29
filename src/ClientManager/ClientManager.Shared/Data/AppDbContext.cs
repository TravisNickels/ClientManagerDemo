using ClientManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientManager.Shared.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Phone> Phones { get; set; } = null!;
}
