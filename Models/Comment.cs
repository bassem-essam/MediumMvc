using System.ComponentModel.DataAnnotations;

namespace MediumMvc.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Foreign keys
        public string PostId { get; set; }
        public Post Post { get; set; }
        
        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }
}
