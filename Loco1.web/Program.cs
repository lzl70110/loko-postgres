using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Loco1.Data;                    // EN: DbContext
using Loco1.Service;                 // EN: Services implementation
using Loco1.Service.Abstractions;    // EN: Service contracts

namespace Loco1.Web
    {
    public class Program
        {
        public static void Main(string[] args)
            {
            // EN: Keep legacy timestamp behavior in Npgsql (safe during transitions)
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            // EN: Host on a fixed HTTP port for easy local run (change if needed)
            // Note: If you start via IIS Express, launchSettings.json controls the port.
            builder.WebHost.UseUrls("http://localhost:5088");

            // ------------------ Configuration & DbContext ------------------

            // EN: Read connection string; ENV overrides appsettings
            var connStr =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection not found.");

            // EN: Normalize rare "server=tcp://host:port" to Npgsql format
            if (connStr.Contains("tcp://", StringComparison.OrdinalIgnoreCase))
                {
                connStr = Regex.Replace(connStr, @"(?i)server\s*=\s*tcp://([^:;]+):(\d+)", "Host=$1;Port=$2");
                }

            // EN: Log sanitized connection string (mask password)
            var sanitized = Regex.Replace(connStr, "(?i)password\\s*=\\s*[^;]*", "Password=***");
            Console.WriteLine($"[CFG] DefaultConnection = {sanitized}");

            // EN: Single DbContext registration (IMPORTANT: keep only this one)
            builder.Services.AddDbContext<LocoDbContext>(opt => opt.UseNpgsql(connStr));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // ------------------ MVC + Localization ------------------

            // EN: Localization (SharedResource-only mode)
            builder.Services.AddLocalization();
            builder.Services
                .AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            // EN: Identity + Roles (dev-friendly password policy)
            builder.Services
                .AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;  // dev
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

            // EN: DI services
            builder.Services.AddScoped<IUserRoleService, UserRoleService>();

            // EN: Razor Pages (Identity UI)
            builder.Services.AddRazorPages();

            // EN: Supported cultures
            CultureInfo[] supportedCultures =
            {
                new("bg-BG"),
                new("en-US")
            };

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new("bg-BG");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            var app = builder.Build();

            // ------------------ DB Migrate -> Seed ------------------

            // EN: Apply EF Core migrations first, then seed roles/admin
            using (var scope = app.Services.CreateScope())
                {
                var services = scope.ServiceProvider;

                var db = services.GetRequiredService<LocoDbContext>();
                db.Database.Migrate(); // EN: create/update schema

                // EN: Seed initial data (roles, admin user, etc.)
                Loco1.Web.Infrastructure.DataSeeder.SeedAsync(services).GetAwaiter().GetResult();
                }

            // ------------------ Middleware pipeline ------------------

            // EN: Localization
            var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            if (app.Environment.IsDevelopment())
                {
                app.UseMigrationsEndPoint();
                // app.UseHttpsRedirection(); // EN: enable if you have a trusted dev HTTPS cert
                }
            else
                {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
                }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();   // EN: must be before Authorization
            app.UseAuthorization();

            // EN: Default MVC route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // EN: Razor Pages for Identity UI
            app.MapRazorPages();

            app.Run();
            }
        }
    }