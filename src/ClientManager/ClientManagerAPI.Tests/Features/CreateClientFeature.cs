using ClientManagerAPI.Data;
using ClientManagerAPI.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using FluentAssertions;

namespace ClientManagerAPI.Tests.Features;

[TestFixture]
internal class CreateClientFeature
{
    PostgreSqlContainer _container = null!;
    AppDbContext _dbContext = null!;
    ClientRepository _clientRepository = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("clientManagerTestDb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _dbContext = new AppDbContext(options);

        _dbContext.Database.EnsureCreated();

    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
        if (_dbContext is not null)
            await _dbContext.DisposeAsync();
    }

    [Test]
    public async Task When_Creating_A_Valid_Client_It_Should_Be_Saved_And_Retrievable()
    {
        // Given a new valid client
        var newClient = new Client
        {
            Id = 1,
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "luke.skywalker@ghmail.com"
        };

        // When saving the client to the database
        var savedClient = await _clientRepository.AddAsync(newClient);

        // Then the client should be retrievable
        savedClient.Should().NotBeNull();
        savedClient.Should().BeEquivalentTo(newClient);
    }
}

