using Microsoft.EntityFrameworkCore;

namespace LisAeroGest.Data
{
    /// <summary>
    /// Marcador vazio, usado apenas para separar as migrações do SQL Server
    /// das migrações do Postgres. Não tem lógica própria.
    /// </summary>
    public class DataContextSqlServer : DataContext
    {
        public DataContextSqlServer(DbContextOptions<DataContextSqlServer> options) : base(options) { }
    }
}