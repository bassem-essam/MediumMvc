using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

public class PostAuthorizationHandler : AuthorizationHandler<PostOwnerRequirement>
{
    private readonly IUserService _userService;
    private readonly ApplicationDbContext _context;
    public PostAuthorizationHandler(IUserService userService, ApplicationDbContext context)
    {
        this._userService = userService;
        this._context = context;
    }
    protected async override Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context, PostOwnerRequirement requirement)
    {
        var completed = Task.CompletedTask;
        var user = await _userService.GetCurrentUserAsync();
        if (context.Resource is HttpContext httpContext)
        {
            var id = httpContext.GetRouteValue("id");
            if (id == null) {
                context.Succeed(requirement);
                return completed;
            }
            
            var post = await _context.Posts.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == id.ToString());
            if (post == null)
            {
                return completed;
            }

            if (post.Author.Id == user.Author.Id)
            {
                context.Succeed(requirement);
            }
        }

        Console.WriteLine($"Author display name: {user.Author.DisplayName}");
        return completed;
    }
}

public class PostOwnerRequirement : IAuthorizationRequirement { }