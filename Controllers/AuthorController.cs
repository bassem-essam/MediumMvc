using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediumMvc.Services;

namespace MediumMvc.Controllers
{
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFollowService _followService;
        private readonly IUserService _userService;

        public AuthorController(ApplicationDbContext context, IFollowService followService, IUserService userService)
        {
            _context = context;
            _followService = followService;
            _userService = userService;
        }


        // GET: @{author}
        [Route("@{username}")]
        public async Task<IActionResult> Details(string username)
        {
            if (username == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .Include(a => a.Posts)
                    .ThenInclude(p => p.Comments)
                .Include(a => a.Posts)
                    .ThenInclude(p => p.Likes)
                .FirstOrDefaultAsync(m => m.Username == username);

            if (author == null)
            {
                return NotFound();
            }

            ViewBag.IsFollowing = false;
            ViewBag.IsSelf = false;

            // Check if current user follows this author
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                ViewBag.IsFollowing = await _followService.IsFollowing(currentUser.AuthorId, author.Id);
                ViewBag.IsSelf = currentUser.AuthorId == author.Id;
            }
            return View(author);
        }
    }
}
