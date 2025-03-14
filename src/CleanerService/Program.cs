using CleanerService;
using CleanerService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) => { 
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); 
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<FileReaderService>(provider =>
            new FileReaderService(context.Configuration["CleanerService:MailDirectory"])
        );
        services.AddSingleton<MessageQueueService>(provider =>
            new MessageQueueService(
                context.Configuration["RabbitMQ:HostName"],
                context.Configuration["RabbitMQ:QueueName"]
            )
        );
        services.AddHostedService<Worker>();
    })
    .Build()
    .Run();