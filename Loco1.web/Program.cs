using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Loco1.Data;                 // EN: DbContext
using Loco1.Service;              // EN: Services implementation
using Loco1.Service.Abstractions; // EN: Service contracts

namespace Loco1.Web
    {
    public class Program
        {
        public static async Task Main(string[] args)
            {
            // EN: Keep legacy timestamp behavior in Npgsql (harmless compatibility switch)
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            // EN: Bind to Render's $PORT in containers; fallback to local dev port.
            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port))
                {
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
                }
            else if (builder.Environment.IsDevelopment())
                {
                builder.WebHost.UseUrls("http://localhost:5088");
                }

            // ------------------ Configuration & DbContext ------------------

            // EN: Read connection string; ENV overrides appsettings
            var connStr =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection not found.");

            // EN: Normalize rare 'server=tcp://host:port' to Npgsql format
            if (connStr.Contains("tcp://", StringComparison.OrdinalIgnoreCase))
                {
                connStr = Regex.Replace(
                    connStr,
                    @"(?i)server\s*=\s*tcp://([^:;]+):(\d+)",
                    "Host=$1;Port=$2");
                }

            // EN: Log sanitized connection string (mask password) – verbatim regex to avoid CS1009
            var sanitized = Regex.Replace(connStr, @"(?i)password\s*=\s*[^;]*", "Password=***");
            Console.WriteLine($"[CFG] DefaultConnection = {sanitized}");

            // EN: Single DbContext registration (keep only this one)
            builder.Services.AddDbContext<LocoDbContext>(opt => opt.UseNpgsql(connStr));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // ------------------ MVC + Localization ------------------

            builder.Services.AddLocalization();
            builder.Services
                .AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            // EN: Identity + Roles (dev-friendly policy)
            builder.Services
                .AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;  // dev only
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

            builder.Services.AddScoped<IUserRoleService, UserRoleService>(); // EN: DI for roles admin
            builder.Services.AddRazorPages();                                // EN: Identity UI

            // EN: Supported cultures
            var supportedCultures = new[]
            {
                new CultureInfo("bg-BG"),
                new CultureInfo("en-US")
            };

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new("bg-BG");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            // EN: Trust reverse proxy headers (Render/NGINX/any proxy)
            builder.Services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // EN: In cloud we don't know proxy IPs; clear to accept all
                opts.KnownNetworks.Clear();
                opts.KnownProxies.Clear();
            });

            var app = builder.Build();

            // ------------------ DB Migrate -> Seed ------------------
            using (var scope = app.Services.CreateScope())
                {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                    {
                    var db = services.GetRequiredService<LocoDbContext>();
                    await db.Database.MigrateAsync(); // EN: create/update schema

                    // EN: Seed initial data (roles, admin user, etc.)
                    await Loco1.Web.Infrastructure.DataSeeder.SeedAsync(services);

                    logger.LogInformation("Startup DB migrate/seed completed.");
                    }
                catch (Exception ex)
                    {
                    // EN: Log full details so we see root cause in Render logs
                    logger.LogCritical(ex, "Startup failure during migrate/seed.");
                    throw;
                    }
                }

            // ------------------ Middleware pipeline ------------------

            app.UseForwardedHeaders();

            // EN: Localization
            var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            if (app.Environment.IsDevelopment())
                {
                app.UseMigrationsEndPoint();
                // app.UseHttpsRedirection(); // enable if you trust Dev HTTPS cert
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

            // EN: Simple health endpoint for cloud checks
            app.MapGet("/healthz", () => Results.Ok("OK"));

            await app.RunAsync();
            }
        }
    }