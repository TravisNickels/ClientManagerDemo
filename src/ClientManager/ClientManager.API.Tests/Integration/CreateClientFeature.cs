using ClientManager.API.Services;
using ClientManager.Shared.Configuration;
using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Data;
using ClientManager.Shared.Messaging;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.RabbitMq;

namespace ClientManager.API.Tests.Integration;

[TestFixture]
internal class CreateClientFeature
{
    RabbitMqContainer _rabbitMqConatiner = null!;
    MessageBrokerFactory _messageBrokerFactory = null!;
    MessagePublisher _messagePublisher = null!;
    RabbitMQConnectionConfiguration _rabbitMqConnectionConfiguration = null!;
    ReadOnlyAppDbContext _readonlyAppDbContext = null!;
    RoutingConvention _routingConvention = null!;
    MessageContextAccessor _messageContextAccessor = null!;
    MessagePublishPipeline _messagePublisherPipeline = null!;
    CreateClient clientCommand = null!;

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

        _rabbitMqConnectionConfiguration = new RabbitMQConnectionConfiguration
        {
            Url = _rabbitMqConatiner.GetConnectionString(),
            AmqpPort = _rabbitMqConatiner.GetMappedPublicPort(5672),
            ManagementPort = _rabbitMqConatiner.GetMappedPublicPort(15672),
            VirtualHost = "/",
            Username = "guest",
            Password = "guest"
        };

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
        });
        var logger = loggerFactory.CreateLogger<MessageValidationMiddleware>();
        var publisherLogger = loggerFactory.CreateLogger<MessagePublisher>();

        var publishMessageList = new List<IMessagePublishMiddleware>();

        _messageBrokerFactory = new MessageBrokerFactory(_rabbitMqConnectionConfiguration);
        _routingConvention = new RoutingConvention();
        _messageContextAccessor = new MessageContextAccessor();
        _messageContextAccessor.GetOrCreateContext();
        _messagePublisherPipeline = new MessagePublishPipeline();
        _messagePublisherPipeline.Use(new MessageValidationMiddleware(logger));
        _messagePublisher = new MessagePublisher(_messageBrokerFactory, _routingConvention, _messageContextAccessor, _messagePublisherPipeline, publisherLogger);

        var options = new DbContextOptionsBuilder<ReadOnlyAppDbContext>().UseInMemoryDatabase("ReadOnlyTestDb").Options;
        _readonlyAppDbContext = new ReadOnlyAppDbContext(options);
    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        if (_rabbitMqConatiner is not null)
            await _rabbitMqConatiner.DisposeAsync();

        if (_messageBrokerFactory is not null)
            await _messageBrokerFactory.DisposeAsync();

        if (_readonlyAppDbContext is not null)
            await _readonlyAppDbContext.DisposeAsync();
    }

    [SetUp]
    public async Task Setup()
    {
        clientCommand = new CreateClient
        {
            Id = Guid.Empty,
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "Luke.Skywalker@gmail.com"
        };

        await _messageBrokerFactory.DeclareAndBindQueue(nameof(CreateClient));
    }

    [Test]
    public async Task When_Creating_Client_The_Service_Should_Enqueue_It_For_Saving()
    {
        // Given a new valid client
        var newClient = clientCommand;

        // When sending the create client message
        var clientService = new ClientService(_messagePublisher, _readonlyAppDbContext);
        var result = await clientService.SendCreateClientMessageAsync(newClient);

        // Then the client should be enqueued successfully
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(clientCommand);
    }

    [Test]
    public async Task When_Sending_Multiple_Clients_The_Service_Should_Enqueue_All_Successfully()
    {
        var clients = new[]
        {
            new CreateClient
            {
                Id = Guid.Empty,
                FirstName = "Anakin",
                LastName = "Skywalker",
                Email = "Anakin.Skywalker@gmail.com"
            },
            new CreateClient
            {
                Id = Guid.Empty,
                FirstName = "Leia",
                LastName = "Organa",
                Email = "Leia.Organa@gmail.com"
            },
            new CreateClient
            {
                Id = Guid.Empty,
                FirstName = "Luke",
                LastName = "Skywalker",
                Email = "luke.Skywalker@gmail.com"
            }
        };

        var clientService = new ClientService(_messagePublisher, _readonlyAppDbContext);

        foreach (var client in clients)
        {
            var result = await clientService.SendCreateClientMessageAsync(client);
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(client);
        }
    }
}
