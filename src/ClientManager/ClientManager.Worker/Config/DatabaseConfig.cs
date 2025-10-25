using ClientManager.Shared.Config;
using dotenv.net;
using Npgsql;

namespace ClientManager.Worker.Config;

public class DatabaseConfig()
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5432;
    public string Database { get; init; } = "clientManagerDB";
    public string Username { get; init; } = "postgres";
    public string Password { get; init; } = "postgres";
    public string Schema { get; init; } = "public";

    /// <summary>
    /// Loads configuration in this order of precedence:
    /// <para>1. Environment variables (including .env)</para>
    /// <para>2. appsettings.json</para>
    /// <para>3. Built-in defaults</para>
    /// </summary>
    /// <param name="basePath"></param>
    /// <returns></returns>
    public static DatabaseConfig Load(string? basePath = null)
    {
        basePath ??= AppContext.BaseDirectory;

        var envPath = ConfigHelper.FindEnvFile(basePath);
        if (File.Exists(envPath))
        {
            DotEnv.Load(new DotEnvOptions(envFilePaths: [envPath]));
        }
        else
        {
            Console.WriteLine($"Warning: .env file not found at {envPath}. Using appsettings.json for environment variables or default database configuration.");
        }

        var configuration = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile("appsettings.json", optional: true, reloadOnChange: false).AddEnvironmentVariables().Build();

        return new DatabaseConfig
        {
            Host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? configuration["DatabaseConfig:POSTGRES_HOST"] ?? "localhost",
            Port = int.TryParse(Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? configuration["DatabaseConfig:POSTGRES_PORT"], out var port) ? port : 5432,
            Database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? configuration["DatabaseConfig:POSTGRES_DATABASE"] ?? "clientManagerDB",
            Username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? configuration["DatabaseConfig:POSTGRES_USER"] ?? "postgres",
            Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? configuration["DatabaseConfig:POSTGRES_PASSWORD"] ?? "postgres",
            Schema = Environment.GetEnvironmentVariable("POSTGRES_SCHEMA") ?? configuration["DatabaseConfig:POSTGRES_SCHEMA"] ?? "public"
        };
    }

    public string ToConnectionString() => ToConnectionStringBuilder().ConnectionString;

    public NpgsqlConnectionStringBuilder ToConnectionStringBuilder() =>
        new()
        {
            Host = Host,
            Port = Port,
            Database = Database,
            Username = Username,
            Password = Password,
            SearchPath = Schema
        };

    public void TestConnection()
    {
        try
        {
            using var connection = new NpgsqlConnection(ToConnectionString());
            connection.Open();
            Console.WriteLine("PostgreSQL connection successful!");
            connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PostgreSQL connection failed: {ex.Message}");
            throw new InvalidOperationException(ex.Message);
        }
    }
}
