using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data
{
    /// <summary>
    /// Marcador vazio, usado apenas para separar as migrações do Postgres
    /// das migrações do SQL Server. Não tem lógica própria.
    /// </summary>
    public class DataContextPostgres : DataContext
    {
        public DataContextPostgres(DbContextOptions<DataContextPostgres> options) : base(options) { }
    }
}