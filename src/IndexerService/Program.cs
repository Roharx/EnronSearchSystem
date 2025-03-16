using Microsoft.Extensions.Configuration;
using IndexerService;

var builder = Host.CreateApplicationBuilder(args);

// Load environment variables from .env, appsettings.json, and system variables
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton<EmailIndexer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();