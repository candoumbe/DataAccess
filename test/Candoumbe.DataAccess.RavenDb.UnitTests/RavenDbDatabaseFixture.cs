using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.ServerWide.Operations;
using Testcontainers.RavenDb;
using Xunit;

namespace Candoumbe.DataAccess.RavenDb.UnitTests;

public class RavenDbDatabaseFixture : IAsyncLifetime
{
    private readonly RavenDbContainer _container;
    public DocumentStore Store { get; private set; }
    /// <summary>
    /// The name of the created database
    /// </summary>
    private string DatabaseName { get; }

    /// <summary>
    /// The connection string used by the current instance.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    public RavenDbDatabaseFixture()
    {
        _container = new RavenDbBuilder()
            .WithImage("ravendb/ravendb:ubuntu-latest-lts")
            .Build();
        DatabaseName = Guid.NewGuid().ToString();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        Store = new DocumentStore
        {
            Urls = [_container.GetConnectionString()],
            Database = DatabaseName
        };
        Store.Initialize();
        CreateDatabaseOperation createDatabaseOperation = new (builder => builder.Regular(DatabaseName));
        await Store.Maintenance.Server.SendAsync(createDatabaseOperation);
    }

    public async Task DisposeAsync()
    {
        DeleteDatabasesOperation deleteDatabaseOperation = new (DatabaseName, hardDelete: true);
        await Store.Maintenance.Server.SendAsync(deleteDatabaseOperation);
        await _container.StopAsync();
    }
}