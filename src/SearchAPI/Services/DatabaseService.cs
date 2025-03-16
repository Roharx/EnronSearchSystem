using System.Text.Json;
using Npgsql;
using SearchAPI.DTO;
using SearchAPI.Interfaces;

namespace SearchAPI.Services;

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("EnronDatabase")!;
    }

    public async Task<object> SearchFilesAsync(string query)
    {
        var results = new List<Dictionary<string, object>>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
        SELECT f.""FileId"", f.""FileName"", o.""Count""
        FROM ""Occurrences"" o
        JOIN ""Words"" w ON o.""WordId"" = w.""WordId""
        JOIN ""Files"" f ON o.""FileId"" = f.""FileId""
        WHERE w.""Text"" ILIKE @query
        ORDER BY o.""Count"" DESC;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("query", $"%{query}%");

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader[i];
            }

            results.Add(row);
        }

        Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));

        return results; // Returns raw data dynamically
    }

    public async Task<FileDto?> GetFileAsync(int fileId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            SELECT ""FileId"", ""FileName""
            FROM ""Files""
            WHERE ""FileId"" = @fileId;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("fileId", fileId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new FileDto
            {
                FileId = reader.GetInt32(0),
                FileName = reader.GetString(1)
            };
        }

        return null;
    }
}
