namespace Candoumbe.DataAccess.EFStore.UnitTests
{
    using Microsoft.Data.Sqlite;

    using System.Threading.Tasks;

    using Xunit;

    /// <summary>
    /// Wraps a connection to an in-memory sqlite database.
    /// </summary>
    public class SqliteDatabaseFixture : IAsyncLifetime
    {
        /// <summary>
        /// Builds a new <see cref="SqliteDatabaseFixture"/> that opens a connection to an in-memory sqlite database
        /// </summary>
        public SqliteDatabaseFixture()
        {
            Connection = new SqliteConnection("Data Source =:memory:");
        }

        /// <summary>
        /// Connection used by the current instance.
        /// </summary>
        public SqliteConnection Connection { get; }

        ///<inheritdoc/>
        public async Task DisposeAsync() => await Connection.CloseAsync().ConfigureAwait(false);

        ///<inheritdoc/>
        public async Task InitializeAsync() => await Connection.OpenAsync().ConfigureAwait(false);
    }

    public class DatabaseFixture<TDatabase> : IAsyncLifetime
    {

    }
}
