using Compartilhei.Application.Abstractions.Identity;

namespace Compartilhei.Api.Identity;

public sealed class GuestSessionAccessor : IGuestSessionAccessor
{
    public const string CookieName = "compartilhei_guest_session";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;

    public GuestSessionAccessor(
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment environment)
    {
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
    }

    public Guid GuestSessionId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                throw new InvalidOperationException(
                    "HTTP context is not available.");
            }

            if (httpContext.Request.Cookies.TryGetValue(
                    CookieName,
                    out var cookieValue)
                && Guid.TryParse(cookieValue, out var guestSessionId)
                && guestSessionId != Guid.Empty)
            {
                return guestSessionId;
            }

            var newGuestSessionId = Guid.NewGuid();

            httpContext.Response.Cookies.Append(
                CookieName,
                newGuestSessionId.ToString(),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_environment.IsDevelopment(),
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    MaxAge = TimeSpan.FromDays(30)
                });

            return newGuestSessionId;
        }
    }
}