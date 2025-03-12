using System.ComponentModel.DataAnnotations;

namespace Database.Models;

public class FileMetadata
{
    [Key]
    public int FileId { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public byte[] Content { get; set; } = Array.Empty<byte>();
}