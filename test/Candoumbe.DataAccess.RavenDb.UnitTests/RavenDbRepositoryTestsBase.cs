using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using Xunit;

namespace Candoumbe.DataAccess.RavenDb.UnitTests;

/// <summary>
/// Base class to extend when writing unit tests for <see cref="EFStore.EntityFrameworkRepository{TEntry, TContext}"/>
/// </summary>
public abstract class RavenDbRepositoryTestsBase : IAsyncLifetime
{
    protected static readonly Faker Faker = new Faker();
    protected RavenDbDatabaseFixture DatabaseFixture { get; }
    protected Session Session { get; }
    
    ///<inheritdoc/>
    public virtual async Task InitializeAsync()
    {
        await DatabaseFixture.InitializeAsync();
    }

    ///<inheritdoc/>
    public virtual async Task DisposeAsync()
    {
        await DatabaseFixture.DisposeAsync();
    }
}