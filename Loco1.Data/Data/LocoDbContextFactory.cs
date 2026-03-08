using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Loco1.Data
{
    /// <summary>
    /// Design-time factory за EF Tools (migrations/update-database).
    /// Чете connection string по същия начин, както приложението:
    /// 1) ENV: ConnectionStrings__DevConnection
    /// 2) appsettings.Development.json / appsettings.json от Web проекта
    /// 3) fallback: ENV/конфиг за DefaultConnection
    /// </summary>
    public class LocoDbContextFactory : IDesignTimeDbContextFactory<LocoDbContext>
    {
        public LocoDbContext CreateDbContext(string[] args)
        {
            // 1) Първо опитай ENV (user-secrets/CI) – DevConnection
            var fromEnvDev = Environment.GetEnvironmentVariable("ConnectionStrings__DevConnection");
            var fromEnvDefault = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            // 2) Зареди конфигурация от Web проекта, за да намерим DevConnection
            //    В design-time текущата директория е Loco1.Data, затова сочим към ../Loco1.Web
            var webDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Loco1.Web"));

            var config = new ConfigurationBuilder()
                .SetBasePath(webDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var fromConfigDev = config.GetConnectionString("DevConnection");
            var fromConfigDefault = config.GetConnectionString("DefaultConnection");

            var connectionString =
                fromEnvDev
                ?? fromConfigDev
                ?? fromEnvDefault
                ?? fromConfigDefault
                ?? throw new InvalidOperationException("No connection string found (DevConnection/DefaultConnection).");

            var options = new DbContextOptionsBuilder<LocoDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new LocoDbContext(options);
        }
    }
}