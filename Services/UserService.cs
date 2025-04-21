using System.Security.Claims;
using MediumMvc.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApplicationDbContext _context;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _context = dbContext;
    }


    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        // Console.WriteLine("User Id: " + userId);
        if (userId == null) return null;
        var user = await _userManager.Users.Include("Author").FirstAsync(u => u.Id == userId.Value);

        // var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
        // if (user == null) {
        //     return null;
        // }

        // var author = await _context.Authors.FindAsync(user.AuthorId);
        // user.Author = author;

        return user;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
    }
}