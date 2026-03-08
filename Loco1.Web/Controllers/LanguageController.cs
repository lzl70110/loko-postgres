using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Loco1.Web.Controllers
    {
    public class LanguageController : Controller
        {
        private static readonly string[] AllowedCultures = new[] { "bg-BG", "en-US" };
        private const string CultureCookieName = ".Loco.Culture";

        // GET: /Language/SetCulture?culture=bg-BG&returnUrl=/
        [HttpGet]
        public IActionResult SetCulture(string culture, string? returnUrl = "/")
            {
            if (!AllowedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase))
                culture = "bg-BG";

            if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = Url.Action("Index", "Home")!;

            Response.Cookies.Append(
                CultureCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                    {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Secure = true,
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax
                    });

            return LocalRedirect(returnUrl);
            }

        // GET: /Language/SetLanguage?culture=bg-BG&returnUrl=/
        [HttpGet]
        public IActionResult SetLanguage(string culture, string? returnUrl = "/") => SetCulture(culture, returnUrl);

        // POST overloads (optional)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetCulturePost(string culture, string? returnUrl = "/") => SetCulture(culture, returnUrl);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetLanguagePost(string culture, string? returnUrl = "/") => SetCulture(culture, returnUrl);
        }
    }