namespace ClientManager.Shared.Messaging;

public interface IRoutingConvention
{
    (string exchange, string RoutingKey) ResolveFor(Type messageType);
}
