using System.Reflection;
using ClientManager.Shared.Messaging;

namespace ClientManager.Worker.Administration;

public static class ServiceCollectionExtensions
{
    public static void AddMessageHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        var handlerInterface = typeof(IMessageHandler<>);

        var registrations =
            from type in assemblies.SelectMany(a => a.GetTypes())
            from iface in type.GetInterfaces()
            where !type.IsAbstract && !type.IsInterface && iface.IsGenericType && iface.GetGenericTypeDefinition() == handlerInterface
            select new { Service = iface, Implementation = type };

        foreach (var r in registrations)
            services.AddScoped(r.Service, r.Implementation);
    }
}
