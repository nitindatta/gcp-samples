using Microsoft.EntityFrameworkCore;
using Demo.Gcp.Entities;
using System.Reflection;

namespace Demo.Gcp.Infrastructure.Data
{

    public class TransactionContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }

        public TransactionContext(DbContextOptions<TransactionContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            //builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        //TODO: Get from configuration at start up

    }
}