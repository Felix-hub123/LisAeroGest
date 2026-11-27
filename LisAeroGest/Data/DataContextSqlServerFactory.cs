using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LisAeroGest.Data
{
    public class DataContextSqlServerFactory
        : IDesignTimeDbContextFactory<DataContextSqlServer>
    {
        public DataContextSqlServer CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable(
                "LISAEROGEST_MIGRATIONS_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "A ligação para as migrations não foi configurada.");
            }

            var optionsBuilder =
                new DbContextOptionsBuilder<DataContextSqlServer>();

            optionsBuilder.UseSqlServer(connectionString);

            return new DataContextSqlServer(optionsBuilder.Options);
        }
    }
}