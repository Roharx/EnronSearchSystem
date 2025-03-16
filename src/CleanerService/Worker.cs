using System.Text;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

namespace CleanerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly string _queueName = "cleaned-emails";
    private readonly string _rabbitMqHost = "message-queue";
    private readonly string _mailDir = "/app/maildir";

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(_mailDir))
        {
            _logger.LogWarning($"maildir not found. Creating it at {_mailDir}");
            Directory.CreateDirectory(_mailDir);
        }

        ConnectionFactory factory = new ConnectionFactory() { HostName = _rabbitMqHost };
        IConnection? connection = null;
        IModel? channel = null;

        int retryAttempts = 5;
        for (int i = 1; i <= retryAttempts; i++)
        {
            try
            {
                _logger.LogInformation($"Attempt {i}: Connecting to RabbitMQ...");
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                break; // Exit loop if connection is successful
            }
            catch (Exception ex)
            {
                _logger.LogError($"RabbitMQ connection failed (Attempt {i}/{retryAttempts}): {ex.Message}");
                await Task.Delay(5000, stoppingToken); // Wait 5 seconds before retrying
            }
        }

        if (connection == null || channel == null)
        {
            _logger.LogError("Failed to connect to RabbitMQ after multiple attempts. Exiting service...");
            return;
        }

        channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
        _logger.LogInformation("Connected to RabbitMQ!");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var files = Directory.EnumerateFiles(_mailDir, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    try
                    {
                        string content = await File.ReadAllTextAsync(file, stoppingToken);
                        string cleanedContent = CleanEmail(content);

                        if (!string.IsNullOrWhiteSpace(cleanedContent))
                        {
                            var body = Encoding.UTF8.GetBytes(cleanedContent);
                            channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null,
                                body: body);
                            _logger.LogInformation($"[x] Cleaned and Sent to RabbitMQ: {file}");
                        }
                        else
                        {
                            _logger.LogWarning($"[!] Skipping empty email after cleaning: {file}");
                        }

                        File.Delete(file);
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError($"Error processing file {file}: {fileEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing files: {ex.Message}");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private string CleanEmail(string rawEmail)
    {
        if (string.IsNullOrWhiteSpace(rawEmail))
            return string.Empty;

        // Normalize line endings
        rawEmail = rawEmail.Replace("\r\n", "\n").Replace("\r", "\n");

        // Find the first empty line (headers end here)
        int headersEnd = rawEmail.IndexOf("\n\n");
        if (headersEnd == -1)
        {
            // If no blank line is found, return the email as is
            return rawEmail.Trim();
        }

        // Extract body after the first empty line
        string emailBody = rawEmail.Substring(headersEnd + 2).Trim();

        // Remove any remaining inline headers (optional)
        emailBody = Regex.Replace(emailBody, @"^(From|To|Subject|Date|Return-Path|Content-Type|Message-ID): .*", "",
            RegexOptions.Multiline);

        return emailBody.Trim();
    }
}