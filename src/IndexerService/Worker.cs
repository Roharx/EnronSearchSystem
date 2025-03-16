using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace IndexerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly EmailIndexer _indexerService;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName = "cleaned-emails";

        public Worker(ILogger<Worker> logger, EmailIndexer indexerService)
        {
            _logger = logger;
            _indexerService = indexerService; // Inject EmailIndexer

            var factory = new ConnectionFactory() { HostName = "message-queue" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
            _logger.LogInformation("Indexer Service Worker started.");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation($"[x] Received email for indexing.");

                try
                {
                    await _indexerService.ProcessEmailAsync(message);
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Message successfully indexed and acknowledged.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing email: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
