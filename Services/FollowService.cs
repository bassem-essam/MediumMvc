using MediumMvc.Data;
using MediumMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace MediumMvc.Services
{
    public interface IFollowService
    {
        Task<bool> ToggleFollow(int followerId, int followedId);
        Task<bool> IsFollowing(int followerId, int followedId);
        Task<int> GetFollowerCount(int authorId);
        Task<int> GetFollowingCount(int authorId);
        Task<List<Author>> GetFollowers(int authorId);
        Task<List<Author>> GetFollowing(int authorId);
    }

    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;

        public FollowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ToggleFollow(int followerId, int followedId)
        {
            if (followerId == followedId)
                return false;

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);

            if (existingFollow != null)
            {
                _context.Follows.Remove(existingFollow);
                await _context.SaveChangesAsync();
                return false;
            }
            else
            {
                var follow = new Follow
                {
                    FollowerId = followerId,
                    FollowedId = followedId
                };
                _context.Follows.Add(follow);
                await _context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> IsFollowing(int followerId, int followedId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);
        }

        public async Task<int> GetFollowerCount(int authorId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowedId == authorId);
        }

        public async Task<int> GetFollowingCount(int authorId)
        {
            return await _context.Follows
                .CountAsync(f => f.FollowerId == authorId);
        }

        public async Task<List<Author>> GetFollowers(int authorId)
        {
            return await _context.Follows
                .Where(f => f.FollowedId == authorId)
                .Include(f => f.Follower)
                .Select(f => f.Follower)
                .ToListAsync();
        }

        public async Task<List<Author>> GetFollowing(int authorId)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == authorId)
                .Include(f => f.Followed)
                .Select(f => f.Followed)
                .ToListAsync();
        }
    }
}