using ClientManager.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientManager.Shared.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Client> Clients { get; set; } = null!;
    public DbSet<Phone> Phones { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Client>()
            .HasData(
                new Client
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    FirstName = "Luke",
                    LastName = "Skywalker",
                    Email = "Luke.Skywalker@gmail.com"
                },
                new Client
                {
                    Id = new Guid("22222222-2222-2222-2222-222222222222"),
                    FirstName = "Han",
                    LastName = "Solo",
                    Email = "Han.Solo@gmail.com"
                },
                new Client
                {
                    Id = new Guid("33333333-3333-3333-3333-333333333333"),
                    FirstName = "Darth",
                    LastName = "Vader",
                    Email = "Darth.Vader@gmail.com"
                },
                new Client
                {
                    Id = new Guid("44444444-4444-4444-4444-444444444444"),
                    FirstName = "Lord",
                    LastName = "Sidious",
                    Email = "Lord.Sidious@gmail.com"
                }
            );
    }
}
