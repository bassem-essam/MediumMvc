using Microsoft.AspNetCore.Identity;

namespace MediumMvc.Areas.Identity.Data;

public class ApplicationUser : IdentityUser 
{
    public int AuthorId { get; set; }
    public Author Author { get; set; }
}