using System.Text.Json;
using ClientManager.API.Services;
using ClientManager.Shared.Configuration;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using FluentAssertions;
using Moq;
using Testcontainers.RabbitMq;

namespace ClientManager.API.Tests.Features;

[TestFixture]
internal class CreateClientFeature
{
    RabbitMqContainer _rabbitMqConatiner = null!;
    MessageBrokerFactory _messageBrokerFactory = null!;
    QueuePublisher _queuePublisher = null!;
    RabbitMQConnectionConfiguration _rabbitMqConnectionConfiguration = null!;
    Client client = null!;

    [OneTimeSetUp]
    public async Task CreateRabbitMqContainer()
    {
        _rabbitMqConatiner = new RabbitMqBuilder()
            .WithUsername("guest")
            .WithPassword("guest")
            .WithExposedPort("5672") // amqp port
            .WithPortBinding("15672", true) // management port
            .WithCleanUp(true)
            .Build();
        await _rabbitMqConatiner.StartAsync();

        _rabbitMqConnectionConfiguration = new RabbitMQConnectionConfiguration(
            url: _rabbitMqConatiner.GetConnectionString(),
            amqpPort: _rabbitMqConatiner.GetMappedPublicPort(5672),
            managementPort: _rabbitMqConatiner.GetMappedPublicPort(15672),
            virtualHost: "/",
            username: "guest",
            password: "guest"
        );

        _messageBrokerFactory = new MessageBrokerFactory(_rabbitMqConnectionConfiguration);
        _queuePublisher = new QueuePublisher(_messageBrokerFactory);
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        if (_rabbitMqConatiner is not null)
            await _rabbitMqConatiner.DisposeAsync();

        if (_messageBrokerFactory is not null)
            await _messageBrokerFactory.DisposeAsync();
    }

    [SetUp]
    public void Setup()
    {
        client = new Client
        {
            Id = Guid.Empty,
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "Luke.Skywalker@gmail.com"
        };
    }

    [Test]
    public async Task When_Creating_Client_With_Empty_Id_The_Service_Should_AutoGenerateId_And_Enqueue_It_For_Saving()
    {
        // Given a new valid client
        var newClient = client;

        // When sending the create client message
        var clientService = new ClientService(_queuePublisher);
        var result = await clientService.SendCreateClientMessage(newClient);

        // Then the client should be enqueued successfully
        result.Should().BeTrue();
        client.Id.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task When_Sending_Multiple_Clients_The_Service_Should_Enqueue_All_Successfully()
    {
        var clients = new[]
        {
            new Client
            {
                Id = Guid.Empty,
                FirstName = "Anakin",
                LastName = "Skywalker",
                Email = "Anakin.Skywalker@gmail.com"
            },
            new Client
            {
                Id = Guid.Empty,
                FirstName = "Leia",
                LastName = "Organa",
                Email = "Leia.Organa@gmail.com"
            },
            new Client
            {
                Id = Guid.Empty,
                FirstName = "Luke",
                LastName = "Skywalker",
                Email = "luke.Skywalker@gmail.com"
            }
        };

        var clientService = new ClientService(_queuePublisher);

        foreach (var client in clients)
        {
            var result = await clientService.SendCreateClientMessage(client);
            result.Should().BeTrue();
        }
    }
}
