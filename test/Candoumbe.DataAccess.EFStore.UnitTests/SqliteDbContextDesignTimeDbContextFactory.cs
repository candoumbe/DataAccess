using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Candoumbe.DataAccess.EFStore.UnitTests;

public class SqliteDbContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<SqliteStore>
{
    ///<inheritdoc/>
    public SqliteStore CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<SqliteStore> optionsBuilder = new DbContextOptionsBuilder<SqliteStore>();
        optionsBuilder.UseSqlite("data source=:memory:");
        return new SqliteStore(optionsBuilder.Options);
    }
}