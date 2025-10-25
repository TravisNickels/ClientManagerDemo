using ClientManager.API.Services;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using FluentAssertions;
using Testcontainers.RabbitMq;

namespace ClientManager.API.Tests.Features;

[TestFixture]
internal class CreateClientFeature
{
    RabbitMqContainer _rabbitMqConatiner = null!;
    MessageBrokerFactory _messageBrokerFactory = null!;

    [OneTimeSetUp]
    public async Task CreateRabbitMqContainer()
    {
        _rabbitMqConatiner = new RabbitMqBuilder()
            .WithUsername("guest")
            .WithPassword("guest")
            .WithExposedPort("5672")
            .WithCleanUp(true)
            .Build();
        await _rabbitMqConatiner.StartAsync();

        var connectionString = _rabbitMqConatiner.GetConnectionString();
        var hostname = _rabbitMqConatiner.Hostname;
        var port = _rabbitMqConatiner.GetMappedPublicPort(5672);

        _messageBrokerFactory = new MessageBrokerFactory(connectionString, port);
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        if (_rabbitMqConatiner is not null)
            await _rabbitMqConatiner.DisposeAsync();
    }

    [Test]
    public async Task When_Creating_A_Valid_Client_The_Service_Should_Enqueue_It_For_Saving()
    {
        // Given a new valid client
        var newClient = new Client
        {
            Id = Guid.Empty,
            FirstName = "Anakin",
            LastName = "Skywalker",
            Email = "Anakin.Skywalker@gmail.com"
        };

        // When sending the create client message
        var clientService = new ClientService(_messageBrokerFactory);
        var result = await clientService.SendCreateClientMessage(newClient);

        // Then the client should be enqueued successfully
        result.Should().BeTrue();
    }
}
