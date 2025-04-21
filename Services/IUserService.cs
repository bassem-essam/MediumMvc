using MediumMvc.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

public interface IUserService
{
    public Task<ApplicationUser?> GetCurrentUserAsync();
    public bool IsAuthenticated();
}