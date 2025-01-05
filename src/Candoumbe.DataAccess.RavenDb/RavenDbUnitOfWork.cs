namespace Candoumbe.DataAccess.RavenDb;

using Candoumbe.DataAccess.Abstractions;

using Raven.Client.Documents.Session;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Raven db implementation of <see cref="IUnitOfWork"/>
/// </summary>
public class RavenDbUnitOfWork : IUnitOfWork
{
    private readonly IAsyncDocumentSession _session;
    private readonly IDictionary<Type, object> _repositories;

    /// <summary>
    /// Builds a new <see cref="RavenDbUnitOfWork"/> instance.
    /// </summary>
    /// <param name="session"></param>
    public RavenDbUnitOfWork(IAsyncDocumentSession session)
    {
        _session = session;
        _repositories = new ConcurrentDictionary<Type, object>();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _session.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public IRepository<TEntry> Repository<TEntry>() where TEntry : class
    {
        IRepository<TEntry> repository;

        if (!_repositories.TryGetValue(typeof(TEntry), out object value))
        {
            repository = value as IRepository<TEntry>;
        }
        else
        {
            repository = new RavenDbRepository<TEntry>(_session);
            _repositories.Add(typeof(TEntry), repository);
        }

        return repository;
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct = default) => await _session.SaveChangesAsync(ct).ConfigureAwait(false);
}