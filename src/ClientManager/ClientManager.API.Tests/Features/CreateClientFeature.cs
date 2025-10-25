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

        client = new Client
        {
            Id = Guid.Empty,
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "Luke.Skywalker@gmail.com"
        };

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
    public async Task When_Creating_A_Valid_Client_With_Empty_Id_The_Service_Should_AutoGenerateId_And_Enqueue_It_For_Saving()
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
    public async Task When_Creating_Client_With_No_First_Name_The_Service_Should_Throw()
    {
        // Given an invalid client (missing first)
        var invalidClient = client;
        invalidClient.FirstName = "";

        var clientService = new ClientService(_queuePublisher);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_No_Last_Name_The_Service_Should_Throw()
    {
        // Given an invalid client (missing first)
        var invalidClient = client;
        invalidClient.LastName = "";

        var clientService = new ClientService(_queuePublisher);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_No_Email_The_Service_Should_Throw()
    {
        // Given an invalid client (missing first)
        var invalidClient = client;
        invalidClient.Email = "";

        var clientService = new ClientService(_queuePublisher);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_QueuePublisher_Fails_The_Service_Should_Throw()
    {
        // Given a mock queue publisher that throws an exception
        var mockPublisher = new Mock<IQueuePublisher>();
        mockPublisher
            .Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Broker down"));

        // When sending the create client message
        var clientService = new ClientService(mockPublisher.Object);
        var newClient = client;

        // Then it should throw an InvalidOperationException with inner exception message
        var ex = await FluentActions.Invoking(() => clientService.SendCreateClientMessage(client)).Should().ThrowAsync<InvalidOperationException>();
        ex.Which.InnerException!.Message.Should().Contain("Broker down");
    }
}
