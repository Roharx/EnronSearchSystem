using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace CleanerService.Services
{
    public class MessageQueueService : IDisposable
    {
        private readonly string _host;
        private readonly string _queueName;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IModel? _channel;

        public MessageQueueService(string host, string queueName)
        {
            _host = host;
            _queueName = queueName;

            _factory = new ConnectionFactory() { HostName = _host };

            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RabbitMQ connection failed: {ex.Message}");
                Dispose();
            }
        }

        public void SendMessage(string fileName, string content)
        {
            if (_channel == null)
            {
                Console.WriteLine("[WARNING] RabbitMQ channel is not available.");
                return;
            }

            var message = new { FileName = fileName, Content = content };
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Ensures messages are saved in case of restart

            _channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: properties,
                body: body
            );

            Console.WriteLine($"[x] Sent: {fileName}");
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to close RabbitMQ connection: {ex.Message}");
            }
        }
    }
}
