using System.Text.Json;
using ClientManager.API.Services;
using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Data;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.RabbitMq;

namespace ClientManager.API.Tests.Unit;

[TestFixture]
internal class ClientServiceTests
{
    CreateClient createClient = null!;
    Mock<IMessagePublisher> mockPublisher = null!;
    ReadOnlyAppDbContext _readonlyAppDbContext = null!;

    [SetUp]
    public void Setup()
    {
        mockPublisher = new Mock<IMessagePublisher>();
        createClient = new CreateClient
        {
            Id = Guid.Empty,
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "Luke.Skywalker@gmail.com"
        };

        var options = new DbContextOptionsBuilder<ReadOnlyAppDbContext>().UseInMemoryDatabase("ReadOnlyTestDb").Options;
        _readonlyAppDbContext = new ReadOnlyAppDbContext(options);
    }

    [TearDown]
    public async Task DisposeAsync()
    {
        if (_readonlyAppDbContext is not null)
            await _readonlyAppDbContext.DisposeAsync();
    }

    [Test]
    public async Task When_Creating_Client_With_No_First_Name_The_Service_Should_Throw()
    {
        // Given an invalid client (missing first name)
        var invalidClient = createClient;
        invalidClient.FirstName = "";

        var clientService = new ClientService(mockPublisher.Object, _readonlyAppDbContext);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_No_Last_Name_The_Service_Should_Throw()
    {
        // Given an invalid client (missing last name)
        var invalidClient = createClient;
        invalidClient.LastName = "";

        var clientService = new ClientService(mockPublisher.Object, _readonlyAppDbContext);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_No_Email_The_Service_Should_Throw()
    {
        // Given an invalid client (missing email)
        var invalidClient = createClient;
        invalidClient.Email = "";

        var clientService = new ClientService(mockPublisher.Object, _readonlyAppDbContext);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_An_Invalid_Email_The_Service_Should_Throw()
    {
        // Given an invalid client (invalid email)
        var invalidClient = createClient;
        invalidClient.Email = "invalid-email-address";

        var clientService = new ClientService(mockPublisher.Object, _readonlyAppDbContext);

        // Then it should throw a domain validation exception
        await FluentActions
            .Invoking(() => clientService.SendCreateClientMessage(invalidClient))
            .Should()
            .ThrowAsync<ArgumentException>()
            .Where(ex => ex.Message.Contains("must have a valid email address"));
    }

    [Test]
    public async Task When_MessagePublisher_Fails_The_Service_Should_Throw()
    {
        // Given a mock queue publisher that throws an exception
        mockPublisher.Setup(p => p.PublishAsync(It.IsAny<IMessage>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Broker down"));

        // When sending the create client message
        var clientService = new ClientService(mockPublisher.Object, _readonlyAppDbContext);
        var newClient = createClient;

        // Then it should throw an InvalidOperationException with inner exception message
        var ex = await FluentActions
            .Invoking(() => clientService.SendCreateClientMessage(newClient))
            .Should()
            .ThrowAsync<Exception>()
            .Where(ex => ex.Message.Contains("Broker down"));
        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<IMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task When_Creating_Valid_Client_The_Service_Should_Serialize_Payload_Correctly()
    {
        // Given a mock queue publisher that captures the published body
        IMessage? capturedBody = null;

        mockPublisher
            .Setup(p => p.PublishAsync(It.IsAny<IMessage>(), It.IsAny<CancellationToken>()))
            .Callback<IMessage, CancellationToken>((message, CancellationToken) => capturedBody = message)
            .Returns(Task.CompletedTask);

        var clientService = new ClientService(mockPublisher.Object, _readonlyAppDbContext);

        var newClient = createClient;

        // When sending the create client message
        await clientService.SendCreateClientMessage(newClient);

        // Then the captured body should deserialize to the expected client
        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<IMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        var deserialized = JsonSerializer.Deserialize<Client>((Stream)capturedBody!);
        deserialized!.FirstName.Should().Be("Luke");
        deserialized.LastName.Should().Be("Skywalker");
        deserialized.Email.Should().Be("Luke.Skywalker@gmail.com");
    }

    [Test]
    public async Task When_Creating_Valid_Client_With_Empty_Id_The_Service_Should_Auto_Generate_Id()
    {
        // Given a mock queue publisher that captures the published body
        IMessage? capturedBody = null;

        mockPublisher
            .Setup(p => p.PublishAsync(It.IsAny<IMessage>(), It.IsAny<CancellationToken>()))
            .Callback<IMessage, CancellationToken>((message, CancellationToken) => capturedBody = message)
            .Returns(Task.CompletedTask);

        var clientService = new ClientService(mockPublisher.Object, _readonlyAppDbContext);

        var newClient = createClient;

        // When sending the create client message
        await clientService.SendCreateClientMessage(newClient);

        // Then the captured body should deserialize to the expected client with a non-empty Id
        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<IMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        var deserialized = JsonSerializer.Deserialize<Client>((Stream)capturedBody!);
        deserialized!.Id.Should().NotBe(Guid.Empty);
    }
}
