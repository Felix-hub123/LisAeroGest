using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LisAeroGest.Data
{
    public class DataContextFactory
        : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable(
                "LISAEROGEST_MIGRATIONS_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "A ligação para as migrations não foi configurada.");
            }

            var optionsBuilder =
                new DbContextOptionsBuilder<DataContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new DataContext(
                optionsBuilder.Options,
                new HttpContextAccessor());
        }
    }
}