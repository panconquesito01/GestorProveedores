using System.Text.Json;
using GestorProveedores.Business.Authentication;
using Microsoft.AspNetCore.DataProtection;

namespace GestorProveedores.WebApp.Services;

public sealed class WebAppSessionService(
    IDataProtectionProvider dataProtectionProvider,
    IHttpContextAccessor httpContextAccessor)
{
    private const string CookieName = "GestorProveedores.Session";
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector("GestorProveedores.WebApp.Session.v1");

    public LoginResponse? GetCurrentSession()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null || !context.Request.Cookies.TryGetValue(CookieName, out var protectedValue))
        {
            return null;
        }

        try
        {
            var json = protector.Unprotect(protectedValue);
            return JsonSerializer.Deserialize<LoginResponse>(json);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void SignIn(HttpContext context, LoginResponse session)
    {
        var json = JsonSerializer.Serialize(session);
        var protectedValue = protector.Protect(json);
        var maxAge = TimeSpan.FromSeconds(session.ExpiresIn);

        context.Response.Cookies.Append(CookieName, protectedValue, new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            MaxAge = maxAge,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps
        });
    }

    public void SignOut(HttpContext context) => context.Response.Cookies.Delete(CookieName);
}