using ClientManager.API.Services;
using ClientManager.Shared.Configuration;
using ClientManager.Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<IMessageBrokerFactory>(bf =>
{
    var connectionConfiguration = RabbitMQConnectionConfiguration.Load();
    return new MessageBrokerFactory(connectionConfiguration);
});
builder.Services.AddScoped<IQueuePublisher, QueuePublisher>();
builder.Services.AddScoped<IClientService, ClientService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
