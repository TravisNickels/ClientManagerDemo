using ClientManager.API.Services;
using ClientManager.Shared.Configuration;
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
    QueuePublisher _queuePublisher = null!;
    RabbitMQConnectionConfiguration _rabbitMqConnectionConfiguration = null!;

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
        var clientService = new ClientService(_queuePublisher);
        var result = await clientService.SendCreateClientMessage(newClient);

        // Then the client should be enqueued successfully
        result.Should().BeTrue();
    }

    [Test]
    public async Task When_Creating_Client_With_No_First_Name_The_Service_Should_Throw()
    {
        // Given an invalid client (missing first)
        var invalidClient = new Client
        {
            Id = Guid.Empty,
            FirstName = "",
            LastName = "Skywalker",
            Email = "skywalker@gmail.com"
        };

        var clientService = new ClientService(_queuePublisher);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_No_Last_Name_The_Service_Should_Throw()
    {
        // Given an invalid client (missing first)
        var invalidClient = new Client
        {
            Id = Guid.Empty,
            FirstName = "Luke",
            LastName = "",
            Email = "luke@gmail.com"
        };

        var clientService = new ClientService(_queuePublisher);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_No_Email_The_Service_Should_Throw()
    {
        // Given an invalid client (missing first)
        var invalidClient = new Client
        {
            Id = Guid.Empty,
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = ""
        };

        var clientService = new ClientService(_queuePublisher);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }
}
