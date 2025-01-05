namespace Candoumbe.DataAccess.EFStore.UnitTests;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class SqliteDbContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqliteDbContext>
{
    ///<inheritdoc/>
    public SqliteDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite("data source=:memory:");
        return new SqliteDbContext(optionsBuilder.Options);
    }
}