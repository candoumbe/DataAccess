namespace Candoumbe.DataAccess.RavenDb.UnitTests;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Xunit;

/// <summary>
/// Base class to extend when writing unit tests for <see cref="RavenDbRepository{T}"/>
/// </summary>
public abstract class RavenDbRepositoryTestsBase : IAsyncLifetime
{
    protected static readonly Faker Faker = new();
    protected IDocumentStore Store { get; }

    protected RavenDbRepositoryTestsBase(RavenDbFixture databaseFixture)
    {
        string databaseName = $"raven-{Guid.NewGuid()}";
        using var _ = new DocumentStore
        {
            Urls = [databaseFixture.Database.GetConnectionString()]
        };

        _.Initialize();

        CreateDatabaseOperation createDatabase = new(new DatabaseRecord(databaseName), 1);
        _.Maintenance.Server.Send(createDatabase);

        Store = new DocumentStore()
        {
            Urls = [databaseFixture.Database.GetConnectionString()],
            Database = databaseName
        };

        Store.Initialize();
    }

    ///<inheritdoc/>
    public Task InitializeAsync()
    {
        Store.Initialize();
        return Task.CompletedTask;
    }

    ///<inheritdoc/>
    public Task DisposeAsync()
    {
        Store.Dispose();
        return Task.CompletedTask;
    }
}