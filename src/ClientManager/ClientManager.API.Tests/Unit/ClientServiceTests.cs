using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ClientManager.API.Services;
using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Data;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

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
}
