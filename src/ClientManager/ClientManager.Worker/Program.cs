using ClientManager.Shared.Configuration;
using ClientManager.Shared.Data;
using ClientManager.Shared.Messaging;
using ClientManager.Worker;
using ClientManager.Worker.Administration;
using ClientManager.Worker.Messaging;
using ClientManager.Worker.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Load .env file if it exists
ConfigurationHelper.LoadDotEnvFile();
builder.Configuration.AddEnvironmentVariables();

// Map configuration sections to strongly typed classes
builder.Services.Configure<PostgresConnectionConfiguration>(builder.Configuration.GetSection("POSTGRES"));
builder.Services.Configure<RabbitMQConnectionConfiguration>(builder.Configuration.GetSection("RABBITMQ"));

Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Logs"));

// Configure Serilog
Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console().WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "worker.log")).CreateLogger();

// Hook Serilog into .NET logging
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IRoutingConvention, RoutingConvention>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();
builder.Services.AddSingleton<IMessageConsumer, RabbitMQMessageConsumer>();
builder.Services.AddSingleton<IMessageBrokerFactory>(sp =>
{
    // Get RabbitMQ connection configuration from DI
    var connectionConfiguration = sp.GetRequiredService<IOptions<RabbitMQConnectionConfiguration>>().Value;
    return new MessageBrokerFactory(connectionConfiguration);
});
builder.Services.AddDbContext<AppDbContext>(
    (sp, options) =>
    {
        // Get Postgres connection configuration from DI
        var postgresConfig = sp.GetRequiredService<IOptions<PostgresConnectionConfiguration>>().Value;
        options.UseNpgsql(postgresConfig.ToConnectionString(), b => b.MigrationsAssembly("ClientManager.Worker"));
    }
);

builder.Services.AddMessageHandlers(AppDomain.CurrentDomain.GetAssemblies());

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var retries = 0;
    const int maxRetries = 10;

    while (retries < maxRetries)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("Database migration successful");
            break;
        }
        catch (Exception ex)
        {
            retries++;
            Console.WriteLine($"Database not ready (attempt {retries}/{maxRetries}): {ex.Message}");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}

host.Run();
