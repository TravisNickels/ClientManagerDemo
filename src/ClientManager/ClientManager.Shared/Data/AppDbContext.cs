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

        modelBuilder.Entity<Client>().HasMany(c => c.Phones).WithOne(p => p.Client).HasForeignKey(p => p.ClientId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Client>()
            .HasData(
                new Client
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    FirstName = "Luke",
                    LastName = "Skywalker",
                    Email = "Luke.Skywalker@gmail.com",
                    IsArchived = false,
                },
                new Client
                {
                    Id = new Guid("22222222-2222-2222-2222-222222222222"),
                    FirstName = "Han",
                    LastName = "Solo",
                    Email = "Han.Solo@gmail.com",
                    IsArchived = false
                },
                new Client
                {
                    Id = new Guid("33333333-3333-3333-3333-333333333333"),
                    FirstName = "Darth",
                    LastName = "Vader",
                    Email = "Darth.Vader@gmail.com",
                    IsArchived = true
                },
                new Client
                {
                    Id = new Guid("44444444-4444-4444-4444-444444444444"),
                    FirstName = "Lord",
                    LastName = "Sidious",
                    Email = "Lord.Sidious@gmail.com",
                    IsArchived = true
                }
            );

        modelBuilder
            .Entity<Phone>()
            .HasData(
                new Phone
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Number = "+1 (111) 111-1111",
                    Type = "Home"
                },
                new Phone
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    ClientId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Number = "+1 (222) 222-2222",
                    Type = "Home"
                },
                new Phone
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    ClientId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Number = "+1 (333) 333-3333",
                    Type = "Home"
                },
                new Phone
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    ClientId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Number = "+1 (444) 444-4444",
                    Type = "Home"
                }
            );
    }
}
