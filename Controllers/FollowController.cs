using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediumMvc.Services;
using System.Security.Claims;

namespace MediumMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;
        private readonly IUserService _userService;

        public FollowController(IFollowService followService, IUserService userService)
        {
            _followService = followService;
            _userService = userService;
        }

        [HttpPost("{followedId}")]
        [Authorize]
        public async Task<IActionResult> ToggleFollow(int followedId)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var followerId = currentUser.AuthorId;
            var isFollowing = await _followService.ToggleFollow(followerId, followedId);

            var followerCount = await _followService.GetFollowerCount(followedId);
            var followingCount = await _followService.GetFollowingCount(followedId);

            return Ok(new
            {
                IsFollowing = isFollowing,
                FollowerCount = followerCount,
                FollowingCount = followingCount
            });
        }

        [HttpGet("{followedId}")]
        public async Task<IActionResult> GetFollowStatus(int followedId)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var isFollowing = false;
            if (currentUser != null)
            {
                isFollowing = await _followService.IsFollowing(currentUser.AuthorId, followedId);
            }
            var followerCount = await _followService.GetFollowerCount(followedId);
            var followingCount = await _followService.GetFollowingCount(followedId);

            return Ok(new
            {
                IsFollowing = isFollowing,
                FollowerCount = followerCount,
                FollowingCount = followingCount
            });
        }

        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            try
            {
                var followers = await _followService.GetFollowers(userId);
                return Ok(followers.Select(f => new
                {
                    f.DisplayName,
                    f.ProfilePictureUrl,
                    f.Bio
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving followers: {ex.Message}");
            }
        }

        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(int userId)
        {
            try
            {
                var following = await _followService.GetFollowing(userId);
                return Ok(following.Select(f => new
                {
                    f.DisplayName,
                    f.ProfilePictureUrl,
                    f.Bio
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving following: {ex.Message}");
            }
        }
    }
}
