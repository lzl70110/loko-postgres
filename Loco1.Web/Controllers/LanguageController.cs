using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Loco1.Web.Controllers
    {
    // English: Handles culture switching via cookie. Supports both /Language/SetCulture and /Language/SetLanguage.
    public class LanguageController : Controller
        {
        // GET: /Language/SetCulture?culture=bg-BG&returnUrl=/
        // EN: Canonical action used by the UI to set the culture cookie and redirect back.
        [HttpGet]
        public IActionResult SetCulture(string culture, string? returnUrl = "/")
            {
            // EN: Validate returnUrl to avoid open-redirects.
            if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
                returnUrl = "/";

            // EN: Write RequestCulture cookie (valid 1 year).
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
            }

        // GET: /Language/SetLanguage?culture=bg-BG&returnUrl=/
        // EN: Backward-compatible alias (some views may call SetLanguage).
        [HttpGet]
        public IActionResult SetLanguage(string culture, string? returnUrl = "/")
            => SetCulture(culture, returnUrl);

        // EN: Optional POST overloads (safe to keep if some forms post here). Not required for link-based switching.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetCulturePost(string culture, string? returnUrl = "/")
            => SetCulture(culture, returnUrl);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetLanguagePost(string culture, string? returnUrl = "/")
            => SetCulture(culture, returnUrl);
        }
    }