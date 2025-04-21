using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediumMvc.Data;
using MediumMvc.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MediumMvc.Areas.Identity.Data;

namespace MediumMvc.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public UserController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> Profile()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
            {
                return NotFound();
            }

            // Load user's posts
            user.Author.Posts = await _context.Posts
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.AuthorId == user.AuthorId)
                .OrderByDescending(p => p.PublishedOn)
                .ToListAsync();

            return View(user.Author);
        }
    }
}