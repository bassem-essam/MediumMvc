using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediumMvc.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IPostService _postService;


        public PostController(ApplicationDbContext context, IUserService userService, IPostService postService)
        {
            _context = context;
            _userService = userService;
            _postService = postService;
        }

        // GET: @{author}/{slug}
        [AllowAnonymous]
        [Route("@{author}/{slug}")]
        public async Task<IActionResult> Details(string author, string slug)
        {
            // Extract ID from slug (last part after last '-')
            var id = slug.Split('-').Last();
            
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Verify author matches
            if (post == null || post.Author?.Username?.ToLower() != author.ToLower())
            {
                return NotFound();
            }
            if (post == null)
            {
                return NotFound();
            }

            var user = await _userService.GetCurrentUserAsync();
            ViewBag.IsAuthor = false;
            ViewBag.AuthorUserName = "";

            if (user != null) {
                ViewBag.IsAuthor = post.AuthorId == user.AuthorId;
                ViewBag.AuthorUserName = user.Author.Username;
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userService.GetCurrentUserAsync();
            var id = await _postService.CreateNewPost(user.Author);

            ViewData["id"] = id;

            return RedirectToAction(nameof(Edit), new { id = id });
        }

        [Authorize(Policy = "PostOwnerPolicy")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            ViewBag.IsNewPost = post.PublishedOn == null;

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PostOwnerPolicy")]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Title,Content")] PostVM postInput)
        {
            var post = await _context.Posts.Include(p => p.Author).FirstAsync(p => p.Id == id);

            if (id != postInput.Id || post == null)
            {
                return NotFound();
            }

            ViewBag.IsNewPost = post.PublishedOn == null;

            if (post.Author == null)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (post.PublishedOn == null)
                    {
                        post.PublishedOn = DateTime.Now;
                    }

                    post.Content = postInput.Content;
                    post.Title = postInput.Title;

                    _context.Posts.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(postInput.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { author = post.Author.Username, slug = post.Slug });
            }

            return View(post);
        }

        // // GET: Post/Delete/5
        // [Authorize(Policy = "PostOwnerPolicy")]
        // public async Task<IActionResult> Delete(string id)
        // {
        //     Console.WriteLine("Deleting post with id: " + id);
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     var post = await _context.Posts
        //         .Include(p => p.Author)
        //         .FirstOrDefaultAsync(m => m.Id == id);
        //     if (post == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(post);
        // }

        // POST: Post/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PostOwnerPolicy")]
        public async Task<IActionResult> Delete(string id)
        {
            Console.WriteLine("Deleting post with id: " + id);
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        private bool PostExists(string id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        [Authorize(Policy = "PostOwnerPolicy")]
        public async Task<IActionResult> RawEdit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            ViewBag.IsNewPost = post.PublishedOn == null;

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PostOwnerPolicy")]
        public async Task<IActionResult> RawEdit(string id, [Bind("Id,Title,Content")] PostVM postInput)
        {
            var post = await _context.Posts.Include(p => p.Author).FirstAsync(p => p.Id == id);

            if (id != postInput.Id || post == null)
            {
                return NotFound();
            }

            ViewBag.IsNewPost = post.PublishedOn == null;

            if (post.Author == null)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (post.PublishedOn == null)
                    {
                        post.PublishedOn = DateTime.Now;
                    }

                    post.Content = postInput.Content;
                    post.Title = postInput.Title;

                    _context.Posts.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(postInput.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { author = post.Author.Username, slug = post.Slug });
            }

            return View(post);
        }
    }
}
