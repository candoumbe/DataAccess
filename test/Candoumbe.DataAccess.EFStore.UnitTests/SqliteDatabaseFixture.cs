namespace Candoumbe.DataAccess.EFStore.UnitTests
{
    using Microsoft.Data.Sqlite;

    using System.Threading.Tasks;

    using Xunit;

    public class SqliteDatabaseFixture : IAsyncLifetime
    {
        private readonly SqliteConnection _connection;

        public SqliteDatabaseFixture()
        {
            _connection = new SqliteConnection("Data Source =:memory:");
        }

        public SqliteConnection Connection => _connection;

        public async Task DisposeAsync() => await _connection.CloseAsync().ConfigureAwait(false);

        public async Task InitializeAsync() => await _connection.OpenAsync().ConfigureAwait(false);
    }
}
