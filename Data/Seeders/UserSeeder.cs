using System.Text.Json;
using MediumMvc.Areas.Identity.Data;
using MediumMvc.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserSeeder> _logger;

    public UserSeeder(IImageService imageService, IFollowService followService, ApplicationDbContext context, ILogger<UserSeeder> logger, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await SeedUsers();
    }
    
    private async Task SeedUsers()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users already seeded - skipping");
            return;
        }

        var seedUsers = GetSeedUsers();

        foreach (var userInfo in seedUsers)
        {
            try
            {
                var author = _context.Authors.FirstOrDefault(a => a.Username == userInfo.Username);
                if (author == null)
                {
                    _logger.LogInformation("Cannot find author {Username} to create a user for", userInfo.Username);
                    continue;
                }

                var user = new ApplicationUser { UserName = userInfo.Email, Email = userInfo.Email, EmailConfirmed = true };
                user.Author = author;

                var result = await _userManager.CreateAsync(user, userInfo.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created user {Username}", userInfo.Username);
                }
                else
                {
                    _logger.LogError("Failed to create user {Username}", userInfo.Username);
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", userInfo.Username);
            }
        }
    }

    private List<UserInfo> GetSeedUsers()
        {
            try
            {
                var jsonPath = Path.Combine("Data", "SeedData", "users.json");
                var jsonData = File.ReadAllText(jsonPath);
                return JsonSerializer.Deserialize<List<UserInfo>>(jsonData) ?? new List<UserInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading seed data from JSON file");
                return new List<UserInfo>();
            }
        }

    public class UserInfo
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
    }
}