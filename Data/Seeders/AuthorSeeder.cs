using System.Text.Json;
using MediumMvc.Services;
using Microsoft.EntityFrameworkCore;

public class AuthorSeeder
{
    private readonly IImageService _imageService;
    private readonly IFollowService _followService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthorSeeder> _logger;

    public AuthorSeeder(IImageService imageService, IFollowService followService, ApplicationDbContext context, ILogger<AuthorSeeder> logger)
    {
        _context = context;
        _imageService = imageService;
        _followService = followService;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedAuthors();
        await SeedFollowRelationships();
    }

    private async Task SeedAuthors()
    {
        if (await _context.Authors.AnyAsync())
        {
            _logger.LogInformation("Authors already seeded - skipping");
            return;
        }

        var seedAuthors = GetSeedAuthors();

        foreach (var authorInfo in seedAuthors)
        {
            try
            {
                var author = new Author
                {
                    Username = authorInfo.Username,
                    DisplayName = authorInfo.DisplayName,
                    Bio = authorInfo.Bio,
                    ProfilePictureUrl = await _imageService.GenerateImage(authorInfo.DisplayName),
                };

                _context.Authors.Add(author);
                _logger.LogInformation("Created author {Username}", authorInfo.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating author {Username}", authorInfo.Username);
            }
        }

        await _context.SaveChangesAsync();
    }

    private List<AuthorInfo> GetSeedAuthors()
    {
        try
        {
            var jsonPath = Path.Combine("Data", "SeedData", "authors.json");
            var jsonData = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<AuthorInfo>>(jsonData) ?? new List<AuthorInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading authors seed data from JSON file");
            return new List<AuthorInfo>();
        }
    }

    private async Task SeedFollowRelationships()
    {
        var authors = await _context.Authors.ToListAsync();
        var seedAuthors = GetSeedAuthors();

        foreach (var authorInfo in seedAuthors)
        {
            var followed = authors.FirstOrDefault(a => a.Username == authorInfo.Username);
            if (followed == null) continue;

            foreach (var followerUsername in authorInfo.Followers)
            {
                var follower = authors.FirstOrDefault(a => a.Username == followerUsername);
                if (follower != null)
                {
                    await _followService.ToggleFollow(follower.Id, followed.Id);
                    _logger.LogInformation("{Follower} now follows {Followed}",
                        follower.Username, followed.Username);
                }
            }
        }
    }

    public class AuthorInfo
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public List<string> Followers { get; set; } = new();
    }
}