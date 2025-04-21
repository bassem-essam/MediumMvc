using MediumMvc.Areas.Identity.Data;

public class Author
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string? Bio { get; set; }
    // public string? ProfilePictureUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }

    // Relationship to ApplicationUser
    // public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    // Navigation property for posts
    public ICollection<Post> Posts { get; set; }

    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();

}
