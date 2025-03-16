using MessageQueue;

var builder = Host.CreateApplicationBuilder(args);

// Register RabbitMQ Message Consumer Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();