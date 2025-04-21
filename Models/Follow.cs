using System.ComponentModel.DataAnnotations;

public class Follow
{
    public int Id { get; set; }

    public int FollowerId { get; set; }
    public Author Follower { get; set; }
    public int FollowedId { get; set; }
    public Author Followed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}