using HamaraCommerce.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace HamaraCommerce.Services;

public static class CommerceSessionValidator
{
    public static async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        // Preserve password-reset/security-stamp revocation before additional account checks.
        await SecurityStampValidator.ValidatePrincipalAsync(context);
        if (context.Principal?.Identity?.IsAuthenticated != true) return;
        var users = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.GetUserAsync(context.Principal);
        if (user == null || !user.IsActive || !user.EmailConfirmed)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    }
}
