namespace SearchAPI.DTO;

public class SearchResultDto
{
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int WordCount { get; set; }
}