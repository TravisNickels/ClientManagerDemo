using ClientManager.API.Services;
using ClientManager.Shared.Configuration;
using ClientManager.Shared.Data;
using ClientManager.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().Enrich.WithProperty("Service", "API"));

// Load .env file if it exists
ConfigurationHelper.LoadDotEnvFile();
builder.Configuration.AddEnvironmentVariables();

// Map configuration sections to strongly typed classes
builder.Services.Configure<PostgresConnectionConfiguration>(builder.Configuration.GetSection("POSTGRES"));
builder.Services.Configure<RabbitMQConnectionConfiguration>(builder.Configuration.GetSection("RABBITMQ"));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddCors();
builder.Services.AddHealthChecks();
builder.Services.AddHostedService<EventForwarder>();
builder.Services.AddScoped<IClientService, ClientService>();

// Context accessor using AsyncLocal to store per-message context
builder.Services.AddSingleton<IMessageContextAccessor, MessageContextAccessor>();

// Core publishing components
builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
builder.Services.AddSingleton<IRoutingConvention, RoutingConvention>();

// Message Publishing middleware
builder.Services.AddSingleton<IMessagePublishMiddleware, MessageValidationMiddleware>();
builder.Services.AddSingleton<IMessagePublishMiddleware, ContextEnrichmentMiddleware>();
builder.Services.AddSingleton<MessagePublishPipeline>(sp =>
{
    var middlewares = sp.GetServices<IMessagePublishMiddleware>();
    var pipeline = new MessagePublishPipeline();

    foreach (var middleware in middlewares.Reverse())
    {
        pipeline.Use(middleware);
    }

    return pipeline;
});

// Broker and database connections
builder.Services.AddSingleton<IMessageBrokerFactory>(sp =>
{
    // Get RabbitMQ connection configuration from DI
    var connectionConfiguration = sp.GetRequiredService<IOptions<RabbitMQConnectionConfiguration>>().Value;
    return new MessageBrokerFactory(connectionConfiguration);
});
builder.Services.AddDbContext<AppDbContext, ReadOnlyAppDbContext>(
    (sp, options) =>
    {
        // Get Postgres connection configuration from DI
        var postgresConfig = sp.GetRequiredService<IOptions<PostgresConnectionConfiguration>>().Value;
        options.UseNpgsql(postgresConfig.ToConnectionString());
    }
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //  Go to http://localhost:5200/swagger/index.html to test the API
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ClientManager API v1");
    });
}

app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notifications");
app.MapControllers();
app.MapHealthChecks("/health");

app.UseCors(options => options.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader().AllowCredentials());

app.Run();
