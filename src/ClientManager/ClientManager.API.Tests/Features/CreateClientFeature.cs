using ClientManager.API.Data;
using ClientManager.API.Models;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using FluentAssertions;
using ClientManager.API.Repositories;
using Moq;
using ClientManager.API.Services;

namespace ClientManagerAPI.Tests.Features;

[TestFixture]
internal class CreateClientFeature
{
    PostgreSqlContainer _container = null!;
    AppDbContext _dbContext = null!;
    ClientRepository _clientRepository = null!;

    [OneTimeSetUp]
    public async Task CreatePostgresContainer()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("clientManagerTestDb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
        if (_dbContext is not null)
            await _dbContext.DisposeAsync();
    }

    [SetUp]
    public void CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
           .UseNpgsql(_container.GetConnectionString())
           .Options;

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
        Func<Task> act = async () => { await _clientRepository.AddAsync(newClient); };

        // Then a DbUpdateException should be thrown
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Test]
    public async Task When_Creating_A_Valid_Client_Then_The_Service_Should_Assign_A_Unique_Id_Before_Saving()
    {
        // Given a new valid client without an Id
        var newClient = new Client
        {
            Id = Guid.Empty,
            FirstName = "Leia",
            LastName = "Organa",
            Email = "leia.organa@gmail.com"
        };

        var repoMock = new Mock<IClientRepository>();
        Client? savedClient = null;

        repoMock
            .Setup(r => r.AddAsync(It.IsAny<Client>()))
            .Callback<Client>(client => savedClient = client)
            .ReturnsAsync((Client client) => client);

        var service = new ClientService(repoMock.Object);

        // When saving the client to the database
        var result = await service.CreateClientAsync(newClient);

        // Then the client should have a unique Id assigned
        savedClient.Should().NotBeNull();
        savedClient.Id.Should().NotBe(Guid.Empty, "the service should assign a GUID before saving");

        result.Id.Should().Be(savedClient.Id);
    }
}

