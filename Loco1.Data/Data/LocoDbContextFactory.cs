using Microsoft.AspNetCore.Http;                     // + for HttpContextAccessor
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Loco1.Data
{
    // Design-time factory used by EF Tools (Add-Migration / Update-Database)
    public class LocoDbContextFactory : IDesignTimeDbContextFactory<LocoDbContext>
    {
        public LocoDbContext CreateDbContext(string[] args)
        {
            // Use the same connection string you have in appsettings for dev
            var connectionString =
    "Host=ep-summer-mode-aluqot0u-pooler.c-3.eu-central-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_JIOt3liAcpS1;SSL Mode=Require;Trust Server Certificate=true";
            var options = new DbContextOptionsBuilder<LocoDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            // Create a minimal accessor for design-time (no real HTTP context)
            // ApplyAudit() will fall back to "system" user when HttpContext is null.
            var httpAccessor = new HttpContextAccessor();

            // Pass both options and accessor to match runtime constructor signature
            return new LocoDbContext(options, httpAccessor);
        }
    }
}