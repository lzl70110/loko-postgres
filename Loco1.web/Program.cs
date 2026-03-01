using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Claims;

using GCommon;
using Loco1.Data;                  // DbContext
using Loco1.Data.Models;           // ApplicationUser
using Loco1.Localizer;             // SharedResource
using Loco1.Repositories;
using Loco1.Repositories.Interfaces;
using Loco1.Service;               // Services implementation
using Loco1.Service.Abstractions;  // Service contracts

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Loco1.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Keep legacy timestamp behavior in Npgsql
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            // Bind port (container / local)
            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port))
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            else if (builder.Environment.IsDevelopment())
                builder.WebHost.UseUrls("http://localhost:5088");

            // Connection string
            var connStr =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection not found.");

            // Normalize rare 'server=tcp://host:port' into Npgsql format
            if (connStr.Contains("tcp://", StringComparison.OrdinalIgnoreCase))
                connStr = Regex.Replace(connStr, @"(?i)server\s*=\s*tcp://([^:;]+):(\d+)", "Host=$1;Port=$2");

            // Log sanitized connection string
            var sanitized = Regex.Replace(connStr, @"(?i)password\s*=\s*[^;]*", "Password=***");
            Console.WriteLine($"[CFG] DefaultConnection = {sanitized}");

            // DbContext
            builder.Services.AddDbContext<LocoDbContext>(opt => opt.UseNpgsql(connStr));

            // Owner options + authorization infra
            builder.Services.Configure<OwnerOptions>(builder.Configuration.GetSection("Seed:Owner"));
            builder.Services.AddAuthorization();
            builder.Services.AddSingleton<IAuthorizationHandler, OwnerOverrideAuthorizationHandler>();
            builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformation>();

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // MVC + Localization
            builder.Services.AddLocalization();
            builder.Services
                .AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

            // Identity
            builder.Services
                .AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequiredLength = 1;
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 0;
                    options.User.RequireUniqueEmail = false;
                    options.Lockout.AllowedForNewUsers = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<LocoDbContext>();

            // App services
            builder.Services.AddScoped<IUserRoleService, UserRoleService>();
            builder.Services.AddScoped<ILocomotiveService, LocomotiveService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();

            // Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<ILocomotiveRepository, LocomotiveRepository>();

            // ---------------------------------------------------------
            //  CULTURES
            // ---------------------------------------------------------
            var supportedCultures = new[]
            {
                new CultureInfo("bg-BG"),
                new CultureInfo("en-US")
            };

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("bg-BG");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                var cookieProvider = new CookieRequestCultureProvider
                {
                    CookieName = ".Loco.Culture"
                };
                options.RequestCultureProviders.Insert(0, cookieProvider);
            });

            // Permission policies (extension in Infrastructure)
            builder.Services.AddPermissionPolicies(
                Perm.Repairs_View, Perm.Repairs_Add, Perm.Repairs_Edit,
                Perm.Expl_View, Perm.Expl_Add, Perm.Expl_Edit,
                Perm.Users_View, Perm.Users_Edit,
                Perm.Roles_View, Perm.Roles_Edit,
                Perm.Loco_View, Perm.Loco_Add, Perm.Loco_Edit, Perm.Loco_Delete
            );

            // Forwarded headers (proxy/CDN)
            builder.Services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                opts.KnownNetworks.Clear();
                opts.KnownProxies.Clear();
            });

            var app = builder.Build();

            // Ensure roles/claims helper (idempotent)
            static async Task EnsureRolesAndClaimsAsync(IServiceProvider sp)
            {
                var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

                // Ensure roles exist
                var rolesToEnsure = new[] { "Owner", "Admin", "Operator" };
                foreach (var rn in rolesToEnsure)
                    if (!await roleManager.RoleExistsAsync(rn))
                        await roleManager.CreateAsync(new IdentityRole(rn));

                // Baseline Admin permission claims
                var admin = await roleManager.FindByNameAsync("Admin");
                if (admin != null)
                {
                    const string ct = "permission";
                    var have = (await roleManager.GetClaimsAsync(admin))
                               .Where(c => c.Type == ct).Select(c => c.Value)
                               .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var want = new[]
                    {
                        Perm.Roles_View, Perm.Roles_Edit,
                        Perm.Users_View, Perm.Users_Edit,
                        Perm.Repairs_View, Perm.Repairs_Add, Perm.Repairs_Edit,
                        Perm.Expl_View, Perm.Expl_Add, Perm.Expl_Edit,
                        Perm.Loco_View, Perm.Loco_Add, Perm.Loco_Edit, Perm.Loco_Delete
                    };

                    foreach (var code in want.Except(have))
                        await roleManager.AddClaimAsync(admin, new Claim(ct, code));
                }
            }

            // Migrations + Seed
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    var db = services.GetRequiredService<LocoDbContext>();
                    await db.Database.MigrateAsync();

                    await EnsureRolesAndClaimsAsync(services);
                    await DataSeeder.SeedAsync(services);

                    logger.LogInformation("Startup DB migrate/seed completed.");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Startup failure during migrate/seed.");
                    throw;
                }
            }

            // Pipeline
            app.UseForwardedHeaders();

            var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();
            app.MapGet("/healthz", () => Results.Ok("OK"));

            await app.RunAsync();
        }
    }
}