using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Candoumbe.DataAccess.EFStore.UnitTests;

/// <summary>
/// Base class to extend when writing unit tests for <see cref="EFStore.EntityFrameworkRepository{TEntry, TContext}"/>
/// </summary>
public abstract class EntityFrameworkRepositoryTestsBase : IAsyncLifetime
{
    protected static readonly Faker Faker = new Faker();
    protected SqliteDatabaseFixture DatabaseFixture { get; }
    protected readonly SqliteStore SqliteStore;

    protected EntityFrameworkRepositoryTestsBase(SqliteDatabaseFixture databaseFixture)
    {
        DbContextOptionsBuilder<SqliteStore> optionsBuilder = new DbContextOptionsBuilder<SqliteStore>();
        optionsBuilder.UseSqlite(databaseFixture.Connection);
        SqliteStore = new SqliteStore(optionsBuilder.Options);
        DatabaseFixture = databaseFixture;
    }

    ///<inheritdoc/>
    public virtual async Task InitializeAsync()
    {
        await DatabaseFixture.InitializeAsync();
        await SqliteStore.Database.EnsureDeletedAsync();
        await SqliteStore.Database.EnsureCreatedAsync();
    }

    ///<inheritdoc/>
    public virtual async Task DisposeAsync()
    {
        await DatabaseFixture.DisposeAsync();
        await SqliteStore.DisposeAsync();
    }
}