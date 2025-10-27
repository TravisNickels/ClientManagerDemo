namespace ClientManager.Shared.Configuration;

public class RabbitMQConnectionConfiguration
{
    public string Url { get; set; } = "amqp://localhost";
    public int AmqpPort { get; set; } = 5672;
    public int ManagementPort { get; set; } = 15672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
