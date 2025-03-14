using CleanerService.Services;

namespace CleanerService
{
    public class Worker : BackgroundService
    {
        private readonly FileReaderService _fileReaderService;
        private readonly MessageQueueService _messageQueueService;
        private readonly ILogger<Worker> _logger;

        public Worker(FileReaderService fileReaderService, MessageQueueService messageQueueService, ILogger<Worker> logger)
        {
            _fileReaderService = fileReaderService;
            _messageQueueService = messageQueueService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleaner Service started.");

            foreach (var (fileName, content) in _fileReaderService.ReadEmails())
            {
                _logger.LogInformation($"Processing file: {fileName}");
                
                _messageQueueService.SendMessage(fileName, content);
                
                _logger.LogInformation($"Sent to message queue: {fileName}");

                await Task.Delay(100, stoppingToken); // Prevent CPU overuse
            }

            _logger.LogInformation("Cleaner Service finished processing emails.");
        }
    }
}