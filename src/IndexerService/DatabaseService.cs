using System.Collections.Concurrent;
using System.Text;
using Npgsql;

namespace IndexerService
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private static readonly ConcurrentDictionary<string, int> _wordCache = new();
        private static readonly SemaphoreSlim _wordInsertLock = new(1, 1);

        public DatabaseService(string databaseUrl)
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');

            _connectionString =
                $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};Pooling=true";
        }

        public NpgsqlConnection CreateConnection() => new(_connectionString);

        public async Task<int> InsertFile(NpgsqlConnection connection, string emailContent)
        {
            const string query = @"
                INSERT INTO ""Files"" (""FileName"", ""Content"") 
                VALUES (@file_name, @content) 
                RETURNING ""FileId"";";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("file_name", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("content", Encoding.UTF8.GetBytes(emailContent));

            return (int)(await cmd.ExecuteScalarAsync() ?? throw new Exception("FileId not returned"));
        }

        public async Task<Dictionary<string, int>> InsertWords(NpgsqlConnection connection, List<string> words)
        {
            var wordIds = new Dictionary<string, int>();

            if (words.Count == 0) return wordIds;

            // Step 1: Filter out words that already exist in cache
            var newWords = new List<string>();
            foreach (var word in words)
            {
                if (_wordCache.TryGetValue(word, out int cachedId))
                {
                    wordIds[word] = cachedId;
                }
                else
                {
                    newWords.Add(word);
                }
            }

            if (newWords.Count == 0) return wordIds; // All words were cached

            await _wordInsertLock.WaitAsync(); // Ensure only one insert batch runs at a time

            try
            {
                // Step 2: Bulk insert new words
                if (newWords.Count > 0)
                {
                    string insertQuery = "INSERT INTO \"Words\" (\"Text\") VALUES " +
                                         string.Join(",", newWords.Select((_, i) => $"(@text{i})")) +
                                         " ON CONFLICT (\"Text\") DO NOTHING RETURNING \"WordId\", \"Text\";";

                    await using var insertCmd = new NpgsqlCommand(insertQuery, connection);
                    for (int i = 0; i < newWords.Count; i++)
                    {
                        insertCmd.Parameters.AddWithValue($"text{i}", newWords[i]);
                    }

                    await using var insertReader = await insertCmd.ExecuteReaderAsync();
                    while (await insertReader.ReadAsync())
                    {
                        string text = insertReader.GetString(1);
                        int id = insertReader.GetInt32(0);
                        wordIds[text] = id;
                        _wordCache[text] = id;
                    }

                    await insertReader.CloseAsync();
                }

                // Step 3: Fetch missing WordIds (words that already existed)
                var missingWords = newWords.Except(wordIds.Keys).ToList();
                if (missingWords.Count > 0)
                {
                    string selectQuery = "SELECT \"WordId\", \"Text\" FROM \"Words\" WHERE \"Text\" = ANY(@words);";
                    await using var selectCmd = new NpgsqlCommand(selectQuery, connection);
                    selectCmd.Parameters.AddWithValue("words", missingWords);

                    await using var selectReader = await selectCmd.ExecuteReaderAsync();
                    while (await selectReader.ReadAsync())
                    {
                        string text = selectReader.GetString(1);
                        int id = selectReader.GetInt32(0);
                        wordIds[text] = id;
                        _wordCache[text] = id;
                    }

                    await selectReader.CloseAsync();
                }
            }
            finally
            {
                _wordInsertLock.Release();
            }

            return wordIds;
        }

        public async Task InsertOccurrences(NpgsqlConnection connection, int fileId, Dictionary<string, int> wordCounts, Dictionary<string, int> wordIds)
        {
            if (wordCounts.Count == 0) return;

            var insertValues = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            int paramIndex = 0;
            foreach (var (word, count) in wordCounts)
            {
                if (wordIds.TryGetValue(word, out int wordId))
                {
                    insertValues.Add($"(@word{paramIndex}, @file{paramIndex}, @count{paramIndex})");
                    parameters.Add(new NpgsqlParameter($"word{paramIndex}", wordId));
                    parameters.Add(new NpgsqlParameter($"file{paramIndex}", fileId));
                    parameters.Add(new NpgsqlParameter($"count{paramIndex}", count));
                    paramIndex++;
                }
            }

            if (insertValues.Count == 0) return; // Nothing to insert

            string query = $@"
                INSERT INTO ""Occurrences"" (""WordId"", ""FileId"", ""Count"") 
                VALUES {string.Join(", ", insertValues)}
                ON CONFLICT (""WordId"", ""FileId"") 
                DO UPDATE SET ""Count"" = ""Occurrences"".""Count"" + EXCLUDED.""Count"";";

            await using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddRange(parameters.ToArray());
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ProcessFile(NpgsqlConnection connection, string emailContent, Dictionary<string, int> wordCounts)
        {
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Step 1: Insert words
                var wordIds = await InsertWords(connection, wordCounts.Keys.ToList());

                // Step 2: Insert file
                int fileId = await InsertFile(connection, emailContent);

                // Step 3: Insert occurrences
                await InsertOccurrences(connection, fileId, wordCounts, wordIds);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
