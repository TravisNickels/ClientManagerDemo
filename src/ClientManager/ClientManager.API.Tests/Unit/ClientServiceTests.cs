using System.Text.Json;
using ClientManager.API.Services;
using ClientManager.Shared.Configuration;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using FluentAssertions;
using Moq;

namespace ClientManager.API.Tests.Unit;

[TestFixture]
internal class ClientServiceTests
{
    Client client = null!;
    Mock<IQueuePublisher> mockPublisher = null!;

    [SetUp]
    public void Setup()
    {
        mockPublisher = new Mock<IQueuePublisher>();
        client = new Client
        {
            Id = Guid.Empty,
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "Luke.Skywalker@gmail.com"
        };
    }

    [Test]
    public async Task When_Creating_Client_With_No_First_Name_The_Service_Should_Throw()
    {
        // Given an invalid client (missing first name)
        var invalidClient = client;
        invalidClient.FirstName = "";

        var clientService = new ClientService(mockPublisher.Object);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_No_Last_Name_The_Service_Should_Throw()
    {
        // Given an invalid client (missing last name)
        var invalidClient = client;
        invalidClient.LastName = "";

        var clientService = new ClientService(mockPublisher.Object);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_No_Email_The_Service_Should_Throw()
    {
        // Given an invalid client (missing email)
        var invalidClient = client;
        invalidClient.Email = "";

        var clientService = new ClientService(mockPublisher.Object);

        // Then it should throw a domain validation exception
        await FluentActions.Invoking(() => clientService.SendCreateClientMessage(invalidClient)).Should().ThrowAsync<ArgumentException>();
    }

    [Test]
    public async Task When_Creating_Client_With_An_Invalid_Email_The_Service_Should_Throw()
    {
        // Given an invalid client (invalid email)
        var invalidClient = client;
        invalidClient.Email = "invalid-email-address";

        var clientService = new ClientService(mockPublisher.Object);

        // Then it should throw a domain validation exception
        await FluentActions
            .Invoking(() => clientService.SendCreateClientMessage(invalidClient))
            .Should()
            .ThrowAsync<ArgumentException>()
            .Where(ex => ex.Message.Contains("must have a valid email address"));
    }

    [Test]
    public async Task When_QueuePublisher_Fails_The_Service_Should_Throw()
    {
        // Given a mock queue publisher that throws an exception
        mockPublisher
            .Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Broker down"));

        // When sending the create client message
        var clientService = new ClientService(mockPublisher.Object);
        var newClient = client;

        // Then it should throw an InvalidOperationException with inner exception message
        var ex = await FluentActions
            .Invoking(() => clientService.SendCreateClientMessage(newClient))
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .Where(ex => ex.InnerException!.Message.Contains("Broker down"));
        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task When_Creating_Valid_Client_The_Service_Should_Serialize_Payload_Correctly()
    {
        // Given a mock queue publisher that captures the published body
        ReadOnlyMemory<byte>? capturedBody = null;

        mockPublisher
            .Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, ReadOnlyMemory<byte>, string, string>((queue, body, exchange, routingKey) => capturedBody = body)
            .Returns(Task.CompletedTask);

        var clientService = new ClientService(mockPublisher.Object);

        var newClient = client;

        // When sending the create client message
        await clientService.SendCreateClientMessage(newClient);

        // Then the captured body should deserialize to the expected client
        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var deserialized = JsonSerializer.Deserialize<Client>(capturedBody!.Value.Span);
        deserialized!.FirstName.Should().Be("Luke");
        deserialized.LastName.Should().Be("Skywalker");
        deserialized.Email.Should().Be("Luke.Skywalker@gmail.com");
    }

    [Test]
    public async Task When_Creating_Valid_Client_With_Empty_Id_The_Service_Should_Auto_Generate_Id()
    {
        // Given a mock queue publisher that captures the published body
        ReadOnlyMemory<byte>? capturedBody = null;

        mockPublisher
            .Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, ReadOnlyMemory<byte>, string, string>((queue, body, exchange, routingKey) => capturedBody = body)
            .Returns(Task.CompletedTask);

        var clientService = new ClientService(mockPublisher.Object);

        var newClient = client;

        // When sending the create client message
        await clientService.SendCreateClientMessage(newClient);

        // Then the captured body should deserialize to the expected client with a non-empty Id
        mockPublisher.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var deserialized = JsonSerializer.Deserialize<Client>(capturedBody!.Value.Span);
        deserialized!.Id.Should().NotBe(Guid.Empty);
    }
}
