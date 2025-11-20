using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace ClientManager.Shared.Messaging;

public class MessageValidationMiddleware(ILogger<MessageValidationMiddleware>? logger) : IMessagePublishMiddleware
{
    readonly ILogger<MessageValidationMiddleware>? _logger = logger;

    public async Task InvokeAsync<T>(T message, MessagePublishDeleagte<T> next, CancellationToken cancellationToken)
        where T : IMessage
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(message);

        bool isValid = Validator.TryValidateObject(message, context, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
            _logger?.LogWarning("Validation failed for {MessageType}: {Errors}", typeof(T).Name, errors);
            throw new ValidationException($"Message validation failed: {errors}");
        }

        _logger?.LogDebug("Message {MessageType} passed validation", typeof(T).Name);

        await next(message, cancellationToken);
    }
}
