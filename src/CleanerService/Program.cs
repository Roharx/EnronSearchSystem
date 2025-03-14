using CleanerService;
using CleanerService.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;

        string mailDir = config["CleanerService:MailDirectory"] ?? "/maildir";
        string rabbitHost = config["RabbitMQ:HostName"] ?? "message-queue";
        string queueName = config["RabbitMQ:QueueName"] ?? "cleaned-emails";

        services.AddSingleton(new FileReaderService(mailDir));
        services.AddSingleton(new MessageQueueService(rabbitHost, queueName));
        services.AddHostedService<Worker>();
    });

builder.Build().Run();