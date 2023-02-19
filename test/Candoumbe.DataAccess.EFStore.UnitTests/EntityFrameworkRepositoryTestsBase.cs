namespace Candoumbe.DataAccess.Tests.Repositories
{
    using Bogus;

    using Candoumbe.DataAccess.EFStore.UnitTests;

    using Microsoft.EntityFrameworkCore;

    using System.Threading.Tasks;

    using Xunit;

    /// <summary>
    /// Base class to extend when writing unit tests for <see cref="EFStore.EntityFrameworkRepository{TEntry, TContext}"/>
    /// </summary>
    public abstract class EntityFrameworkRepositoryTestsBase : IAsyncLifetime
    {
        protected static readonly Faker Faker = new Faker();
        protected SqliteDatabaseFixture DatabaseFixture { get; }
        protected readonly SqliteDbContext SqliteDbContext;

        protected EntityFrameworkRepositoryTestsBase(SqliteDatabaseFixture databaseFixture)
        {
            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite(databaseFixture.Connection);
            SqliteDbContext = new SqliteDbContext(optionsBuilder.Options);
            DatabaseFixture = databaseFixture;
        }

        ///<inheritdoc/>
        public async Task InitializeAsync()
        {
            await DatabaseFixture.InitializeAsync().ConfigureAwait(false);
            await SqliteDbContext.Database.EnsureDeletedAsync().ConfigureAwait(false);
            await SqliteDbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async Task DisposeAsync()
        {
            await DatabaseFixture.DisposeAsync().ConfigureAwait(false);
            await SqliteDbContext.DisposeAsync().ConfigureAwait(false);
        }
    }
}