using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

public static class AuthorizationPolicies
{
    public static void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(
            "PostOwnerPolicy",
            policy => policy.Requirements.Add(new PostOwnerRequirement()));
    }
}