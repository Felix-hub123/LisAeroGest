using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LisAeroGest.Data
{
    public class DataContextPostgresFactory
        : IDesignTimeDbContextFactory<DataContextPostgres>
    {
        public DataContextPostgres CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            // Para esta Factory queremos especificamente
            // a configuração PostgreSQL.
            //
            // Não carregamos appsettings.Development.json,
            // porque esse ficheiro contém a ligação SQL Server.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString =
                configuration.GetConnectionString(
                    "DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "A connection string PostgreSQL " +
                    "'DefaultConnection' não foi encontrada.");
            }

            var optionsBuilder =
                new DbContextOptionsBuilder<DataContextPostgres>();

            optionsBuilder.UseNpgsql(
                connectionString,
                options =>
                {
                    options.MigrationsAssembly(
                        "LisAeroGest");

                    options.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        "public");
                });

            return new DataContextPostgres(
                optionsBuilder.Options);
        }
    }
}