using System.Text.RegularExpressions;
using MediumMvc.Models;

public class Post
{
    public string Id { get; set; }
    public string? Title { get; set; }
    public string? Slug
    {
        get
        {
            if (string.IsNullOrEmpty(Title))
                return $"post-{Id}";

            var slug = Regex.Replace(Title, @"[^a-zA-Z0-9\s-]", " ");

            // Convert multiple spaces/hyphens to single hyphen
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim();

            // Convert to lowercase
            slug = slug.ToLower();

            // Trim length to 60 chars (Medium's limit is ~60 for slug part)
            slug = slug.Length <= 60 ? slug : slug.Substring(0, 60);

            return $"{slug}-{Id}";
        }
    }
    public string? Content { get; set; }
    public DateTime? PublishedOn { get; set; }

    public string CreatedAt => PublishedOn?.ToShortDateString() ?? DateTime.Now.ToShortDateString();

    public string Excerpt
    {
        get
        {
            if (string.IsNullOrEmpty(Content))
                return string.Empty;

            // Strip HTML tags using regex
            var strippedContent = Regex.Replace(Content, "<.*?>", string.Empty);

            // Strip HTML entities using regex
            strippedContent = Regex.Replace(strippedContent, "&.*?;", string.Empty);

            // Get the first 20 words
            var words = strippedContent.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var first20Words = words.Take(20);

            // Join the words back into a single string
            return string.Join(" ", first20Words);
        }
    }

    // Relationship to Author
    public int AuthorId { get; set; }
    public Author Author { get; set; }

    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    // Helper property to get total claps
    public int TotalClaps => Likes.Sum(l => l.ClapCount);
}
