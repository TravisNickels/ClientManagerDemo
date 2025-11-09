namespace ClientManager.Shared.Messaging;

public class RoutingConvention : IRoutingConvention
{
    public (string exchange, string RoutingKey) ResolveFor(Type messageType)
    {
        var exchange = "client-manager";
        var routingKey = messageType.Name;
        return (exchange, routingKey);
    }
}
