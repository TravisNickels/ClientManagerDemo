using ClientManager.Shared.Data;
using ClientManager.Shared.Models;
using ClientManager.Worker.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace ClientManager.Worker.Tests.Features;

[TestFixture]
internal class CreateClientFeature
{
    PostgreSqlContainer _postgresContainer = null!;
    AppDbContext _dbContext = null!;
    ClientRepository _clientRepository = null!;

    [OneTimeSetUp]
    public async Task CreatePostgresContainer()
    {
        _postgresContainer = new PostgreSqlBuilder().WithDatabase("clientManagerTestDb").WithUsername("postgres").WithPassword("postgres").Build();

        await _postgresContainer.StartAsync();
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        if (_postgresContainer is not null)
            await _postgresContainer.DisposeAsync();
        if (_dbContext is not null)
            await _dbContext.DisposeAsync();
    }

    [SetUp]
    public void CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(_postgresContainer.GetConnectionString()).Options;

        _dbContext = new AppDbContext(options);

        _dbContext.Database.EnsureCreated();

        _clientRepository = new ClientRepository(_dbContext);
    }

    [TearDown]
    public async Task ClearDatabase()
    {
        if (_dbContext is not null)
            await _dbContext.DisposeAsync();
    }

    [Test]
    public async Task When_Creating_A_Valid_Client_Then_It_Should_Be_Saved_And_Returned()
    {
        // Given a new valid client
        var newClient = new Client
        {
            Id = Guid.Empty,
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

    [Test]
    public async Task When_Creating_A_Client_Without_A_FirstName_It_Should_Throw_DbUpdateException()
    {
        // Given a new client without an email
        var newClient = new Client
        {
            Id = Guid.Empty,
            FirstName = null,
            LastName = "Solo",
            Email = "han.solo@gmail.com"
        };

        // When saving the client to the database
        Func<Task> act = async () =>
        {
            await _clientRepository.AddAsync(newClient);
        };

        // Then a DbUpdateException should be thrown
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
