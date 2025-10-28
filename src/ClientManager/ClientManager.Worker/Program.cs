using ClientManager.Shared.Configuration;
using ClientManager.Shared.Messaging;
using ClientManager.Worker;
using ClientManager.Worker.Configuration;
using ClientManager.Worker.Data;
using ClientManager.Worker.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

// Load .env file if it exists
ConfigurationHelper.LoadDotEnvFile();
builder.Configuration.AddEnvironmentVariables();

// Map configuration sections to strongly typed classes
builder.Services.Configure<PostgresConnectionConfiguration>(builder.Configuration.GetSection("POSTGRES"));
builder.Services.Configure<RabbitMQConnectionConfiguration>(builder.Configuration.GetSection("RABBITMQ"));

builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IMessageConsumer, RabbitMQMessageConsumer>();
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
        options.UseNpgsql(postgresConfig.ToConnectionString());
    }
);

var host = builder.Build();
host.Run();
