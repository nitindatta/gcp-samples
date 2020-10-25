using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Demo.Gcp.Infrastructure.Data
{
 class TransactionContextDesignTimeContextFactory : IDesignTimeDbContextFactory<TransactionContext>
    {
        public TransactionContext CreateDbContext(string[] args)
        {
            // Build config            
            IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), ""))
            .AddJsonFile("appsettings.json")
            .Build();
            var optionsBuilder = new DbContextOptionsBuilder<TransactionContext>();
            var connectionString = config.GetConnectionString("DemoDb");
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Demo.Gcp"));

            return new TransactionContext(optionsBuilder.Options);
        }
    }
}