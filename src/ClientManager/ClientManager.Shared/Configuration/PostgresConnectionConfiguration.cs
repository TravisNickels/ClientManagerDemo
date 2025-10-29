using Npgsql;

namespace ClientManager.Shared.Configuration;

public class PostgresConnectionConfiguration()
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5432;
    public string Database { get; init; } = "clientManagerDB";
    public string Username { get; init; } = "postgres";
    public string Password { get; init; } = "postgres";
    public string Schema { get; init; } = "public";

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
