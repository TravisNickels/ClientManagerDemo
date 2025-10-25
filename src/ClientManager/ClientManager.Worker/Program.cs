using ClientManager.Worker;
using ClientManager.Worker.Configuration;
using ClientManager.Worker.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
var DbConfig = DatabaseConnectionConfiguration.Load();

//DbConfig.TestConnection();

builder.Services.AddHostedService<Worker>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(DbConfig.ToConnectionString())
);

var host = builder.Build();
host.Run();
