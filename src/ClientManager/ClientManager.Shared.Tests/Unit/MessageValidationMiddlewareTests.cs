using System.ComponentModel.DataAnnotations;
using ClientManager.Shared.Contracts.Commands;
using ClientManager.Shared.Messaging;
using ClientManager.Shared.Tests.Fakes;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace ClientManager.Shared.Tests.Unit;

[TestFixture]
internal class MessageValidationMiddlewareTests
{
    CreateClient createClient = null!;

    [SetUp]
    public void Setup()
    {
        createClient = new CreateClient
        {
            Id = Guid.Empty,
            FirstName = "Luke",
            LastName = "Skywalker",
            Email = "Luke.Skywalker@gmail.com"
        };
    }

    [Test]
    public async Task MessageValidationMiddleware_WhenClientIsVaild_ShouldPass()
    {
        var logger = new FakeLogger<MessageValidationMiddleware>();

        var middleware = new MessageValidationMiddleware(logger);
        var ct = new CancellationToken();

        await middleware.InvokeAsync(createClient, (msg, _) => Task.CompletedTask, ct);

        logger.Logs.Should().Contain(log => log.Level == LogLevel.Debug && log.Message.Contains("passed validation"));
    }

    [Test]
    public async Task MessageValidationMiddleware_WhenClientIsMissingFirstName_ShouldWarnWithAnnotation()
    {
        var logger = new FakeLogger<MessageValidationMiddleware>();

        var middleware = new MessageValidationMiddleware(logger);
        var cancellationToken = new CancellationToken();

        createClient.FirstName = string.Empty;

        var ex = await FluentActions
            .Invoking(() => middleware.InvokeAsync(createClient, (msg, _) => Task.CompletedTask, cancellationToken))
            .Should()
            .ThrowAsync<ValidationException>()
            .Where(ex => ex.Message.Contains("Message validation failed"));

        logger.Logs.Should().Contain(log => log.Level == LogLevel.Warning && log.Message.Contains("Client must have a first name"));
    }
}
