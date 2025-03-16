
using Npgsql;

namespace IndexerService
{
    public class EmailIndexer
    {
        private readonly ILogger<EmailIndexer> _logger;
        private readonly DatabaseService _databaseService;
        private readonly WordProcessor _wordProcessor;

        public EmailIndexer(ILogger<EmailIndexer> logger, IConfiguration configuration)
        {
            _logger = logger;

            // Load DB Connection
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") ??
                              configuration.GetSection("Database")["ConnectionString"] ??
                              throw new InvalidOperationException("DATABASE_URL is not set.");

            _databaseService = new DatabaseService(databaseUrl);
            _wordProcessor = new WordProcessor();
        }

        public async Task ProcessEmailAsync(string emailContent)
        {
            const int maxRetries = 3;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                attempt++;
                try
                {
                    await using var connection = _databaseService.CreateConnection();
                    await connection.OpenAsync();
                    await using var transaction = await connection.BeginTransactionAsync();

                    // Step 1: Insert File
                    int fileId = await _databaseService.InsertFile(connection, emailContent);
                    _logger.LogInformation($"Inserted file with FileId {fileId}");

                    // Step 2: Extract & Insert Words
                    var words = _wordProcessor.ExtractWords(emailContent);
                    var wordIds = await _databaseService.InsertWords(connection, words.Keys.ToList());

                    // Step 3: Insert Occurrences
                    await _databaseService.InsertOccurrences(connection, fileId, words, wordIds);

                    await transaction.CommitAsync();
                    _logger.LogInformation("Email indexed successfully.");
                    return;
                }
                catch (PostgresException ex) when (ex.SqlState == "40P01") // Deadlock detected
                {
                    _logger.LogWarning($"Deadlock detected, retrying {attempt}/{maxRetries}...");
                    await Task.Delay(100 * attempt);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing email: {ex.Message}");
                    throw;
                }
            }

            _logger.LogError("Max retries reached, skipping email.");
        }
    }
}
