namespace Candoumbe.DataAccess.RavenDb.UnitTests;

using System;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.ServerWide.Operations;
using Testcontainers.RavenDb;
using Xunit;

/// <summary>
/// Wraps a connection Raven database.
/// </summary>
public class RavenDbFixture : IAsyncLifetime
{
    
    /// <summary>
    /// Builds a new <see cref="RavenDb"/> that opens a connection to an in-memory sqlite database
    /// </summary>
    public RavenDbFixture()
    {
        Database = new RavenDbBuilder()
            .WithImage("ravendb/ravendb:ubuntu-latest-lts")
            .Build();
    }

    /// <summary>
    /// Connection used by the current instance.
    /// </summary>
    public RavenDbContainer Database { get; }

    ///<inheritdoc/>
    public Task DisposeAsync() => Task.CompletedTask;

    ///<inheritdoc/>
    public async Task InitializeAsync() => await Database.StartAsync().ConfigureAwait(false);
}