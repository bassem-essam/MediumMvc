using System.Text.Json;
using MediumMvc.Models;
using MediumMvc.Services;
using Microsoft.EntityFrameworkCore;

public class PostSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PostSeeder> _logger;

    public PostSeeder(ApplicationDbContext context, ILogger<PostSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedPosts();
    }

    private List<PostInfo> GetSeedPosts()
    {
        try
        {
            var jsonPath = Path.Combine("Data", "SeedData", "posts.json");
            var jsonData = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<PostInfo>>(jsonData) ?? new List<PostInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading posts seed data from JSON file");
            return new List<PostInfo>();
        }
    }

    private async Task SeedPosts()
    {
        if (await _context.Posts.AnyAsync())
        {
            _logger.LogInformation("Posts already seeded - skipping");
            return;
        }

        var seedPosts = GetSeedPosts();

        Console.WriteLine("Hello we did it! count: " + seedPosts.Count());

        foreach (var postInfo in seedPosts)
        {
            _logger.LogInformation("Creating post {}", JsonSerializer.Serialize(postInfo));

            try
            {
                var author = await _context.Authors
                    .FirstOrDefaultAsync(a => a.Username == postInfo.Author);

                if (author == null)
                {
                    _logger.LogWarning("Author {Author} not found for post {Title}",
                        postInfo.Author, postInfo.Title);
                    continue;
                }

                var htmlPath = Path.Combine("Data", "SeedData", "HTML", $"{postInfo.Html}.html");
                var content = File.ReadAllText(htmlPath);

                var post = new Post
                {
                    Id = Guid.NewGuid().ToString().Substring(0, 8),
                    Title = postInfo.Title,
                    Content = content,
                    AuthorId = author.Id,
                    PublishedOn = DateTime.Parse(postInfo.Published)
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                foreach (var commentInfo in postInfo.Comments)
                {
                    var commentAuthor = await _context.Authors
                        .FirstOrDefaultAsync(a => a.Username == commentInfo.Author);

                    if (commentAuthor != null)
                    {
                        var comment = new Comment
                        {
                            Content = commentInfo.Message,
                            AuthorId = commentAuthor.Id,
                            PostId = post.Id
                        };
                        _context.Comments.Add(comment);
                    }
                }

                if (postInfo.ClapCount > 0)
                {
                    await _context.Likes.AddAsync(new Like { PostId = post.Id, AuthorId = 1, ClapCount = postInfo.ClapCount });
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Created post {Title} by {Author}",
                    postInfo.Title, postInfo.Author);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post {Title}", postInfo.Title);
            }
        }
    }

    public class PostInfo
    {
        public string Author { get; set; }
        public string Html { get; set; }
        public string Title { get; set; }
        public string Published { get; set; }
        public List<CommentInfo> Comments { get; set; } = new();
        public int ClapCount { get; set; }
    }

    public class CommentInfo
    {
        public string Author { get; set; }
        public string Message { get; set; }
    }

}