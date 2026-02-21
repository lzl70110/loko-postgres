using Loco1.Data;
using Loco1.Service.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using Loco1.Service;



namespace Loco1.Web
    {
    public class Program
        {
        public static void Main(string[] args)
            {
            // EN: Compatibility switch for older DateTime behavior in Npgsql during transition
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var builder = WebApplication.CreateBuilder(args);

            // Connection string (PostgreSQL)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not found.");

            // EF Core + PostgreSQL provider
            builder.Services.AddDbContext<LocoDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Localization (SharedResource-only mode)
            builder.Services.AddLocalization();
            builder.Services
                .AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

            builder.Services.AddScoped<IUserRoleService, UserRoleService>(); // Admin role management service

            // Razor Pages support (required for Identity UI)
            builder.Services.AddRazorPages();

            // Identity (dev-friendly + roles)
            builder.Services
                .AddDefaultIdentity<IdentityUser>(options =>
                {
                    // Sign-in
                    options.SignIn.RequireConfirmedAccount = false; // dev friendly; set true in production

                    // Password policy - relaxed (dev)
                    options.Password.RequiredLength = 1;
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredUniqueChars = 0;

                    // Optional dev tweaks
                    options.User.RequireUniqueEmail = false;
                    options.Lockout.AllowedForNewUsers = false;
                })
                .AddRoles<IdentityRole>()                 // <-- enable roles support
                .AddEntityFrameworkStores<LocoDbContext>();

            // Supported cultures
            CultureInfo[] supportedCultures =
            {
                new CultureInfo("bg-BG"),
                new CultureInfo("en-US")
            };

            // Request localization options
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new("bg-BG");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            var app = builder.Build();

            // Seed roles + admin + user (run once at startup)
            using (var scope = app.Services.CreateScope())
                {
                var services = scope.ServiceProvider;
                // Fully-qualified call to avoid extra using in this file
                Loco1.Web.Infrastructure.DataSeeder.SeedAsync(services).GetAwaiter().GetResult();
                }

            // Enable localization
            var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            // Pipeline
            if (app.Environment.IsDevelopment())
                {
                app.UseMigrationsEndPoint();
                }
            else
                {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // must be before Authorization
            app.UseAuthorization();

            // Default MVC route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Razor Pages for Identity UI
            app.MapRazorPages();

            app.Run();
            }
        }
    }