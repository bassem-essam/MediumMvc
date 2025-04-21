// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MediumMvc.Areas.Identity.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediumMvc.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        private readonly IImageService _imageService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IImageService imageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _imageService = imageService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            public string Bio { get; set; }

            [Display(Name = "Display Name")]
            public string DisplayName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile ProfilePicture { get; set; }
        }
        public string ProfilePictureUrl { get; set; }

        private async Task LoadAsync(ApplicationUser user, Author author)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Bio = author.Bio,
                DisplayName = author.DisplayName,
                PhoneNumber = phoneNumber
            };

            ProfilePictureUrl = author.ProfilePictureUrl;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var author = await _context.Authors.FindAsync(user.AuthorId);

            await LoadAsync(user, author);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // var user = await _context.Users.Find
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var author = await _context.Authors.FindAsync(user.AuthorId);

            if (!ModelState.IsValid)
            {
                await LoadAsync(user, author);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.DisplayName != user.Author.DisplayName)
            {
                author.DisplayName = Input.DisplayName;
            }

            if (Input.Bio != user.Author.Bio)
            {
                author.Bio = Input.Bio;
            }

            user.Author = author;

            if (Input.ProfilePicture != null)
            {
                try
                {
                    var url = await _imageService.UploadImage(Input.ProfilePicture, author.Username);
                    if (url != "" && url != "/images/default_profile.svg")
                    {
                        _imageService.DeleteImage(author.ProfilePictureUrl);
                    }

                    author.ProfilePictureUrl = url;
                }
                catch (Exception e)
                {
                    return BadRequest("Failed to upload image, error: " + e.Message);
                }
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
