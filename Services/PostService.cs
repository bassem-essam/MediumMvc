using Microsoft.EntityFrameworkCore;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;

    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateNewPost(Author author)
    {
        var id = Guid.NewGuid().ToString().Substring(0, 8);
        var post = new Post() { Title = "Untitled", Author = author, Id = id };

        if (author.Posts == null)
        {
            author.Posts = new List<Post>();
        }


        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        author.Posts.Add(post);
        _context.Authors.Update(author);
        await _context.SaveChangesAsync();
        return id;
    }

    public async Task<Post> GetPostAsync(string id) => await _context.Posts.Include(p => p.Author).Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Post>> GetPostsAsync(int pageNumber = 1, int pageSize = 10) 
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.PublishedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<Post>> GetFeed(Author author, int pageNumber = 1, int pageSize = 10)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.Author.Followers.Any(f => f.FollowerId == author.Id))
            .OrderByDescending(p => p.PublishedOn)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<int> GetFeedCount(Author author) {
        return await _context.Posts
            .Where(p => p.Author.Followers.Any(f => f.FollowerId == author.Id))
            .CountAsync();
    }
    
    public async Task<int> GetPostsCount() => await _context.Posts.CountAsync();
}
