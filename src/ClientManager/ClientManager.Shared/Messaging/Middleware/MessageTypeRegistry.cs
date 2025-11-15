using System.Reflection;

namespace ClientManager.Shared.Messaging;

public class MessageTypeRegistry
{
    readonly Assembly[] _assembliesToScan;
    public IReadOnlyDictionary<string, Type> MessageTypeCache;

    public MessageTypeRegistry(Assembly[]? assemblies = null)
    {
        _assembliesToScan = assemblies ?? AppDomain.CurrentDomain.GetAssemblies();
        MessageTypeCache = DiscoverMessageTypes();
    }

    Dictionary<string, Type> DiscoverMessageTypes() =>
        _assembliesToScan
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .ToDictionary(t => t.Name, t => t);

    public Type? GetMessageTypeByName(string messageTypeName) => MessageTypeCache.TryGetValue(messageTypeName, out var messageType) ? messageType : null;
}
