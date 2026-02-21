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
                "Host=localhost;Port=5432;Database=loco_db;Username=loco_user;Password=701109;Include Error Detail=true;Pooling=true;Maximum Pool Size=50";

            var options = new DbContextOptionsBuilder<LocoDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new LocoDbContext(options);
            }
        }
    }