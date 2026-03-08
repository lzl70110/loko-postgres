// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Loco1.Localizer;                           // <-- marker type SharedResource
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Loco1.Data.Models;
namespace Loco1.Web.Areas.Identity.Pages.Account
    {
    public class RegisterModel : PageModel
        {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser>_userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer<SharedResource> _L;

        public RegisterModel(
            UserManager<ApplicationUser>userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            ILogger<RegisterModel> logger,
            IStringLocalizer<SharedResource> localizer)
            {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
            _L = localizer;
            }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
            {
            // EN: Display and validation messages are localized via SharedResource keys.

            [Required(ErrorMessage = "Required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Required")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password length error")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            public string ConfirmPassword { get; set; }
            }

        public async Task OnGetAsync(string returnUrl = null)
            {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
            {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
                {
                _logger.LogInformation("User created a new account with password.");

                // EN: Assign default 'User' role on registration (single-role policy downstream).
                const string defaultRole = "User";

                // ensure role exists
                if (!await _roleManager.RoleExistsAsync(defaultRole))
                    {
                    var createRole = await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                    if (!createRole.Succeeded)
                        {
                        ModelState.AddModelError(string.Empty, _L["Failed to ensure default role."]);
                        foreach (var e in createRole.Errors)
                            ModelState.AddModelError(string.Empty, $"{e.Code}: {e.Description}");
                        return Page();
                        }
                    }

                // assign default role
                var addToRole = await _userManager.AddToRoleAsync(user, defaultRole);
                if (!addToRole.Succeeded)
                    {
                    ModelState.AddModelError(string.Empty, _L["Default role assignment failed."]);
                    foreach (var e in addToRole.Errors)
                        ModelState.AddModelError(string.Empty, $"{e.Code}: {e.Description}");
                    return Page();
                    }

                // EN: email confirmation (localized subject + body)
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId, code, returnUrl },
                    protocol: Request.Scheme);

                var subject = _L["Confirm your email"].Value;
                var body = string.Format(
                    _L["Please confirm your account by clicking here"].Value + " {0}",
                    $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>{_L["clicking here"].Value}</a>"
                );

                await _emailSender.SendEmailAsync(Input.Email, subject, body);

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                else
                    {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                    }
                }

            // EN: bubble Identity errors through ModelState (localized by Identity or left as-is)
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
            }

        private ApplicationUser CreateUser()
            {
            try
                {
                return Activator.CreateInstance<ApplicationUser>();
                }
            catch
                {
                throw new InvalidOperationException(
                    $"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
                }
            }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
            {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException(_L["The default UI requires a user store with email support."]);

            return (IUserEmailStore<ApplicationUser>)_userStore;
            }
        }
    }