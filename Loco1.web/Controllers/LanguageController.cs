using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Loco1.Web.Controllers
    {
    public class LanguageController : Controller
        {
        // Sets the UI culture cookie
        public IActionResult SetCulture(string culture, string returnUrl)
            {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
            }
        }
    }