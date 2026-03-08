using System.Globalization;
using System.Text.RegularExpressions;

using Loco1.Localizer;             // SharedResource
using Loco1.Data;                  // DbContext
using Loco1.Data.Models;           // ApplicationUser
using Loco1.Service;               // Services implementation
using Loco1.Service.Abstractions;  // Service contracts

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
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

            // ---------------------------------------------------------
            //  BIND PORT (Render / Dev)
            // ---------------------------------------------------------
            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port))
            {
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            }
            else if (builder.Environment.IsDevelopment())
            {
                builder.WebHost.UseUrls("http://localhost:5088");
            }

            // ---------------------------------------------------------
            //  CONNECTION STRING (DevConnection -> DefaultConnection fallback)
            // ---------------------------------------------------------
            var connStr =
                Environment.GetEnvironmentVariable("ConnectionStrings__DevConnection")
                ?? builder.Configuration.GetConnectionString("DevConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DevConnection/DefaultConnection not found.");

            // Normalize rare 'server=tcp://host:port' patterns
            if (connStr.Contains("tcp://", StringComparison.OrdinalIgnoreCase))
            {
                connStr = Regex.Replace(
                    connStr,
                    @"(?i)server\s*=\s*tcp://([^:;]+):(\d+)",
                    "Host=$1;Port=$2");
            }

            // Log sanitized connection string (hide password)
            var sanitized = Regex.Replace(connStr, @"(?i)password\s*=\s*[^;]*", "Password=***");
            Console.WriteLine($"[CFG] Using connection = {sanitized}");

            // DbContext
            builder.Services.AddDbContext<LocoDbContext>(opt => opt.UseNpgsql(connStr));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // ---------------------------------------------------------
            //  MVC + LOCALIZATION (SharedResource-only)
            // ---------------------------------------------------------
            builder.Services.AddLocalization();

            builder.Services
                .AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });

            // ---------------------------------------------------------
            //  IDENTITY (ApplicationUser)
            // ---------------------------------------------------------
            builder.Services
                .AddDefaultIdentity<ApplicationUser>(options =>
                {
                    // Dev-friendly defaults; adjust for prod as needed
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

            builder.Services.AddScoped<IUserRoleService, UserRoleService>();
            builder.Services.AddRazorPages(); // Identity UI

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
                options.DefaultRequestCulture = new("bg-BG");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            // ---------------------------------------------------------
            //  FORWARDED HEADERS
            // ---------------------------------------------------------
            builder.Services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                opts.KnownNetworks.Clear();
                opts.KnownProxies.Clear();
            });

            var app = builder.Build();

            // ---------------------------------------------------------
            //  MIGRATIONS + DATA SEED
            // ---------------------------------------------------------
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var db = services.GetRequiredService<LocoDbContext>();
                    await db.Database.MigrateAsync();

                    await Loco1.Web.Infrastructure.DataSeeder.SeedAsync(services);

                    logger.LogInformation("Startup DB migrate/seed completed.");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "Startup failure during migrate/seed.");
                    throw;
                }
            }

            // ---------------------------------------------------------
            //  MIDDLEWARE PIPELINE
            // ---------------------------------------------------------
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