using System.Collections;
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

        ValidateRecursively(message, validationResults, []);

        if (validationResults.Count != 0)
        {
            var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
            _logger?.LogWarning("Validation failed for {MessageType}: {Errors}", typeof(T).Name, errors);
            throw new ValidationException($"Message validation failed: {errors}");
        }
        _logger?.LogDebug("Message {MessageType} passed validation", typeof(T).Name);
        await next(message, cancellationToken);
    }

    static void ValidateRecursively(object obj, List<ValidationResult> results, HashSet<object> visited)
    {
        if (obj == null || visited.Contains(obj))
            return;

        visited.Add(obj);

        var context = new ValidationContext(obj);
        Validator.TryValidateObject(obj, context, results, validateAllProperties: true);

        var properties = obj.GetType().GetProperties().Where(p => p.CanRead);

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);

            if (value == null)
                continue;

            // Prevent loops and string missclassification
            if (value is IEnumerable enumerable && value is not string)
            {
                foreach (var element in enumerable)
                    ValidateRecursively(element, results, visited);
            }
            else if (!property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
            {
                ValidateRecursively(value, results, visited);
            }
        }
    }
}
