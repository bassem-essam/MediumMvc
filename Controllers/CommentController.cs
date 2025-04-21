using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediumMvc.Models;
using MediumMvc.Services;

namespace MediumMvc.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public CommentController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var comment = await _context.Comments.Include(c => c.Post).ThenInclude(p => p.Author).FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            var user = await _userService.GetCurrentUserAsync();
            if (user == null || comment.AuthorId != user.AuthorId)
            {
                return Forbid();
            }

            return View(comment);
        }

        public class CommentInput {
            public int Id { get; set; }
            public string Content { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] CommentInput commentInput)
        {
            if (id != commentInput.Id)
            {
                return BadRequest("Comment ID mismatch");
            }

            var comment = await _context.Comments.Include(c => c.Post).ThenInclude(p => p.Author).FirstOrDefaultAsync(c => c.Id == id);
            // var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var user = await _userService.GetCurrentUserAsync();
            if (user == null || comment.AuthorId != user.AuthorId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    comment.Content = commentInput.Content;
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Post", new { slug = comment.Post.Slug, author = comment.Post.Author.Username });
            }

            return View(comment);
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Post)
                .ThenInclude(p => p.Author)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            // Allow deletion by comment author or post author
            if (comment.AuthorId != user.AuthorId && comment.Post.AuthorId != user.AuthorId)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Post", new { slug = comment.Post.Slug, author = comment.Post.Author.Username });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string postId, string content)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var comment = new Comment
            {
                Content = content,
                PostId = postId,
                AuthorId = user.AuthorId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var post = await _context.Posts.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) {
                return NotFound();
            }

            return RedirectToAction("Details", "Post", new { slug = post.Slug, author = post.Author.Username });
        }
    }
}
