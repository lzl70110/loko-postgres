using Loco1.Data.Models;  // ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Loco1.Web.Areas.Identity.Pages.Account
    {
    public class LogoutModel : PageModel
        {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
            {
            _signInManager = signInManager;
            _logger = logger;
            }

        public async Task<IActionResult> OnPostAsync(string returnUrl )
            {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            if (!string.IsNullOrWhiteSpace(returnUrl))
                return LocalRedirect(returnUrl);

            // Force a new GET so the identity is refreshed
            return RedirectToPage();
            }
        }
    }