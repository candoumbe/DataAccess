namespace Candoumbe.DataAccess.Tests.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class SqliteDbContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqliteDbContext>
    {
        ///<inheritdoc/>
        public SqliteDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite("data source=:memory:");
            return new SqliteDbContext(optionsBuilder.Options);
        }
    }
}
