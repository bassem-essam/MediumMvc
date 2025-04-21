// using Microsoft.EntityFrameworkCore;

// namespace MediumMvc.Services
// {
//     public interface IFollowService
//     {
//         Task<bool> ToggleFollow(int AuthorId);
//         // Task<int> GetFollowers(int AuthorId);
//     }

//     public class FollowService : IFollowService
//     {
//         private readonly ApplicationDbContext _context;

//         public FollowService(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         public async Task<bool> ToggleFollow(Author author, int followedAuthorId)
//         {
//             var follow = await _context.Follows
//                 .FirstAsync(f => f.FollowerId == author.Id && f.FollowedId == followedAuthorId);
//             if (follow != null)
//             {
//                 _context.Entry(follow).State = EntityState.Deleted;
//             }
//             else
//             {
//                 _context.Follows.Add(new Follow { FollowerId = author.Id, FollowedId = followedAuthorId });
//             }

//             await _context.SaveChangesAsync();
//             return follow != null;
//         }


//         public async Task<bool> HasUserFollowed(int authorId, int otherAuthorId)
//         {
//             return false;
//         }
//     }
// }