using Microsoft.EntityFrameworkCore;

namespace MediumMvc.Services
{
    public interface ILikeService
    {
        Task<int> ToggleLike(string postId, int AuthorId);
        Task<int> GetTotalClaps(string postId);
        Task<bool> HasUserLiked(string postId, int AuthorId);
    }

    public class LikeService : ILikeService
    {
        private readonly ApplicationDbContext _context;

        public LikeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> ToggleLike(string postId, int AuthorId)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.AuthorId == AuthorId);

            if (existingLike != null)
            {
                _context.Entry(existingLike).State = EntityState.Deleted;
            }
            else
            {
                // Add new like
                var like = new Like
                {
                    PostId = postId,
                    AuthorId = AuthorId,
                };
                _context.Likes.Add(like);
            }

            await _context.SaveChangesAsync();
            return await GetTotalClaps(postId);
        }

        public async Task<int> GetTotalClaps(string postId)
        {
            return await _context.Likes
                .Where(l => l.PostId == postId)
                .SumAsync(l => l.ClapCount);
        }

        public async Task<bool> HasUserLiked(string postId, int AuthorId)
        {
            return await _context.Likes
                .AnyAsync(l => l.PostId == postId && l.AuthorId == AuthorId);
        }
    }
}