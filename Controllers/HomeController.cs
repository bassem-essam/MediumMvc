using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MediumMvc.Models;
using Microsoft.AspNetCore.Components;

namespace MediumMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPostService _postService;
    private readonly IUserService _userService;

    public HomeController(ILogger<HomeController> logger, IPostService postService, IUserService userService)
    {
        _logger = logger;
        _postService = postService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(string feed, int pageNumber = 1)
    {
        ViewBag.HasNextPage = false;

        if (!User.Identity.IsAuthenticated)
        {
            return View(new List<Post>());
        }

        var pageSize = 3;

        if (feed == "following") {
            var user = await _userService.GetCurrentUserAsync();
            var feedCount = await _postService.GetFeedCount(user.Author);

            if (feedCount > pageNumber * pageSize)   {
                ViewBag.HasNextPage = true;
            }

            return View(await _postService.GetFeed(user.Author, pageNumber, pageSize));
        } else {
            var feedCount = await _postService.GetPostsCount();

            if (feedCount > pageNumber * pageSize)   {
                ViewBag.HasNextPage = true;
            }

            return View(await _postService.GetPostsAsync(pageNumber, pageSize));
        }
    }

    public async Task<IActionResult> Hello()
    {
        var user = await _userService.GetCurrentUserAsync();
        return Ok(new { User = user } );
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
