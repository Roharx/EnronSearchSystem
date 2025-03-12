using System.ComponentModel.DataAnnotations;

namespace Database.Models;

public class Word
{
    [Key]
    public int WordId { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;
}