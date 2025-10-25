using dotenv.net;
using Microsoft.Extensions.Configuration;

namespace ClientManager.Shared.Configuration;

public class RabbitMQConnectionConfiguration(string url, int amqpPort, int managementPort, string virtualHost, string username, string password)
{
    const string defaultUrl = "amqp://localhost";
    const string defaultVirtualHost = "/";
    const string defaultUsername = "guest";
    const string defaultPassword = "guest";
    const int defaultPort = 5672;
    const int defaultManagementPort = 15672;

    public string Url { get; } = url;
    public int AmqpPort { get; } = amqpPort;
    public int ManagementPort { get; } = managementPort;
    public string VirtualHost { get; } = virtualHost;
    public string Username { get; } = username;
    public string Password { get; } = password;

    /// <summary>
    /// Loads configuration in this order of precedence:
    /// <para>1. Environment variables (including .env)</para>
    /// <para>2. appsettings.json</para>
    /// <para>3. Built-in defaults</para>
    /// </summary>
    /// <param name="basePath"></param>
    /// <returns></returns>
    public static RabbitMQConnectionConfiguration Load(string? basePath = null)
    {
        basePath ??= AppContext.BaseDirectory;

        var envPath = ConfigurationHelper.FindEnvFile(basePath);
        if (File.Exists(envPath))
        {
            DotEnv.Load(new DotEnvOptions(envFilePaths: [envPath]));
        }
        else
        {
            Console.WriteLine($"Warning: .env file not found at {envPath}. Using appsettings.json for environment variables or default broker configuration.");
        }

        var configuration = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile("appsettings.json", optional: true, reloadOnChange: false).AddEnvironmentVariables().Build();

        return new RabbitMQConnectionConfiguration(
            url: Environment.GetEnvironmentVariable("RABBITMQ_URL") ?? configuration["BrokerConfig:RABBITMQ_URL"] ?? defaultUrl,
            amqpPort: int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_AMQP_PORT") ?? configuration["BrokerConfig:RABBITMQ_AMQP_PORT"], out var amqpPort) ? amqpPort : defaultPort,
            managementPort: int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_MANAGEMENT_PORT") ?? configuration["BrokerConfig:RABBITMQ_MANAGEMENT_PORT"], out var managementPort) ? managementPort : defaultManagementPort,
            virtualHost: Environment.GetEnvironmentVariable("RABBITMQ_VIRTUAL_HOST") ?? configuration["BrokerConfig:RABBITMQ_VIRTUAL_HOST"] ?? defaultVirtualHost,
            username: Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? configuration["BrokerConfig:RABBITMQ_USERNAME"] ?? defaultUsername,
            password: Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? configuration["BrokerConfig:RABBITMQ_PASSWORD"] ?? defaultPassword
        );
    }
}
