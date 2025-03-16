using SearchAPI.DTO;

namespace SearchAPI.Interfaces;

public interface IDatabaseService
{
    Task<object> SearchFilesAsync(string query); // Changed from List<SearchResultDto> to object
    Task<FileDto?> GetFileAsync(int fileId);
}