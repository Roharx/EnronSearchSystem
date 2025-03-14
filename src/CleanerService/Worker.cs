using System;
using System.Threading;
using System.Threading.Tasks;
using CleanerService.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

public class Worker : BackgroundService
{
    private readonly FileReaderService _fileReader;
    private readonly MessageQueueService _messageQueue;

    public Worker(IConfiguration config)
    {
        string mailDir = config["MailDir"];
        string rabbitHost = config["RabbitMQ:Host"];
        string queueName = config["RabbitMQ:QueueName"];

        _fileReader = new FileReaderService(mailDir);
        _messageQueue = new MessageQueueService(rabbitHost, queueName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Cleaner Service started.");

        foreach (var (fileName, content) in _fileReader.ReadEmails())
        {
            _messageQueue.SendMessage(fileName, content);
        }

        Console.WriteLine("Processing complete.");
        await Task.Delay(1000, stoppingToken); // Prevent instant shutdown
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _messageQueue.Dispose();
        return base.StopAsync(cancellationToken);
    }
}