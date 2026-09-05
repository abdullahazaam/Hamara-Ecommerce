using System.Security.Claims;
using HamaraCommerce.Models;
using HamaraCommerce.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace HamaraCommerce.Tests;

public class SessionValidationTests
{
    [Fact]
    public async Task SecurityStampRejectionIsPreservedWithoutReacceptingTheUser()
    {
        var stamps = new Mock<ISecurityStampValidator>();
        stamps.Setup(s => s.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>()))
            .Callback<CookieValidatePrincipalContext>(c => c.RejectPrincipal()).Returns(Task.CompletedTask);
        using var services = new ServiceCollection().AddSingleton(stamps.Object).BuildServiceProvider();
        var context = MakeContext(services);
        await CommerceSessionValidator.ValidateAsync(context);
        Assert.Null(context.Principal);
        stamps.Verify(s => s.ValidateAsync(context), Times.Once);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task InactiveOrUnverifiedAccountIsRejectedAfterStampValidation(bool active, bool confirmed)
    {
        var stamps = new Mock<ISecurityStampValidator>();
        stamps.Setup(s => s.ValidateAsync(It.IsAny<CookieValidatePrincipalContext>())).Returns(Task.CompletedTask);
        var users = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
        users.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new ApplicationUser { IsActive = active, EmailConfirmed = confirmed });
        var auth = new Mock<IAuthenticationService>();
        auth.Setup(a => a.SignOutAsync(It.IsAny<HttpContext>(), IdentityConstants.ApplicationScheme, It.IsAny<AuthenticationProperties?>())).Returns(Task.CompletedTask);
        using var services = new ServiceCollection().AddSingleton(stamps.Object).AddSingleton(users.Object).AddSingleton(auth.Object).BuildServiceProvider();
        var context = MakeContext(services);
        await CommerceSessionValidator.ValidateAsync(context);
        Assert.Null(context.Principal);
        stamps.Verify(s => s.ValidateAsync(context), Times.Once);
        auth.Verify(a => a.SignOutAsync(context.HttpContext, IdentityConstants.ApplicationScheme, It.IsAny<AuthenticationProperties?>()), Times.Once);
    }

    private static CookieValidatePrincipalContext MakeContext(IServiceProvider services)
    {
        var http = new DefaultHttpContext { RequestServices = services };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-1") }, "Identity"));
        return new CookieValidatePrincipalContext(http,
            new AuthenticationScheme(IdentityConstants.ApplicationScheme, null, typeof(CookieAuthenticationHandler)),
            new CookieAuthenticationOptions(), new AuthenticationTicket(principal, IdentityConstants.ApplicationScheme));
    }
}
