using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

public class Occurrence
{
    [Key]
    [Column(Order = 1)]
    public int WordId { get; set; }
    [Key]
    [Column(Order = 2)]
    public int FileId { get; set; }
    [Required]
    public int Count { get; set; }
    
    [ForeignKey("WordId")]
    public Word Word { get; set; } = null!;

    [ForeignKey("FileId")]
    public FileMetadata File { get; set; } = null!;
}