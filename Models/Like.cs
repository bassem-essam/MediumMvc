using System.ComponentModel.DataAnnotations;

public class Like
{
    [Key]
    public int Id { get; set; }

    public int AuthorId { get; set; }
    public Author Author { get; set; }

    public string PostId { get; set; }
    public Post Post { get; set; }

    public int ClapCount { get; set; } = 1; // Default to 1 clap
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}