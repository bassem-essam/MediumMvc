using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediumMvc.Services;
using System.Security.Claims;

namespace MediumMvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly IUserService _userService;

        public LikeController(ILikeService likeService, IUserService userService)
        {
            _likeService = likeService;
            _userService = userService;
        }

        [HttpPost("{postId}")]
        public async Task<IActionResult> ToggleLike(string postId)
        {
            var user = await _userService.GetCurrentUserAsync();
            var totalClaps = await _likeService.ToggleLike(postId, user.AuthorId);
            return Ok(new { TotalClaps = totalClaps });
        }

        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLikeInfo(string postId)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetCurrentUserAsync();

            var hasLiked = user != null && await _likeService.HasUserLiked(postId, user.AuthorId);
            var totalClaps = await _likeService.GetTotalClaps(postId);

            return Ok(new
            {
                TotalClaps = totalClaps,
                HasLiked = hasLiked
            });
        }
    }
}