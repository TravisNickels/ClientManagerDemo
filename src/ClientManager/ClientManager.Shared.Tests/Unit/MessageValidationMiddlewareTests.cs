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

    record NoAnnotationsMessage : IMessage;

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
    public async Task Should_PassValidation_When_MessageIsValid()
    {
        var logger = new FakeLogger<MessageValidationMiddleware>();

        var middleware = new MessageValidationMiddleware(logger);
        var ct = new CancellationToken();

        await middleware.InvokeAsync(createClient, (msg, _) => Task.CompletedTask, ct);

        logger.Logs.Should().Contain(log => log.Level == LogLevel.Debug && log.Message.Contains("passed validation"));
    }

    [TestCase("", "Skywalker", "Luke.Skywalker@gmail.com", "first name")]
    [TestCase("Luke", "", "Luke.Skywalker@gmail.com", "last name")]
    [TestCase("Luke", "Skywalker", "not-an-email", "valid email")]
    public async Task Should_LogWarning_When_RequiredFieldIsMissing(string firstName, string lastName, string email, string expectedError)
    {
        var logger = new FakeLogger<MessageValidationMiddleware>();
        var middleware = new MessageValidationMiddleware(logger);
        var cancellationToken = new CancellationToken();

        var message = new CreateClient
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        var ex = await FluentActions
            .Invoking(() => middleware.InvokeAsync(message, (_, _) => Task.CompletedTask, cancellationToken))
            .Should()
            .ThrowAsync<ValidationException>()
            .Where(ex => ex.Message.Contains("Message validation failed"));

        logger.Logs.Should().Contain(log => log.Level == LogLevel.Warning && log.Message.Contains(expectedError, StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task Should_Not_InvokeNext_When_ValidationFails()
    {
        var logger = new FakeLogger<MessageValidationMiddleware>();
        var middleware = new MessageValidationMiddleware(logger);
        var cancellationToken = new CancellationToken();

        createClient.FirstName = "";

        bool nextCalled = false;

        await FluentActions
            .Invoking(
                () =>
                    middleware.InvokeAsync(
                        createClient,
                        (_, _) =>
                        {
                            nextCalled = true;
                            return Task.CompletedTask;
                        },
                        cancellationToken
                    )
            )
            .Should()
            .ThrowAsync<ValidationException>();

        nextCalled.Should().BeFalse("next delegate should not be called when validation fails");
    }

    [Test]
    public async Task Should_InvokeNext_When_MessageIsValid()
    {
        var logger = new FakeLogger<MessageValidationMiddleware>();
        var middleware = new MessageValidationMiddleware(logger);
        var cancellationToken = new CancellationToken();

        bool nextCalled = false;

        await middleware.InvokeAsync(
            createClient,
            (msg, _) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            cancellationToken
        );

        nextCalled.Should().BeTrue();
    }

    [Test]
    public async Task Should_PassValidation_When_MessageHasNoValidationAttributes()
    {
        var logger = new FakeLogger<MessageValidationMiddleware>();
        var middleware = new MessageValidationMiddleware(logger);
        var cancellationToken = new CancellationToken();

        var message = new NoAnnotationsMessage();

        await middleware.Invoking(m => m.InvokeAsync(message, (_, _) => Task.CompletedTask, cancellationToken)).Should().NotThrowAsync();
    }
}
