using Candoumbe.DataAccess.Abstractions;
using Candoumbe.DataAccess.Repositories;
using DataFilters;
using Optional;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;

namespace Candoumbe.DataAccess.RavenDb;

/// <summary>
/// RavenDb implementation of <see cref="IRepository{TEntry}"/>
/// </summary>
/// <typeparam name="T">Type of items that the repository will manage.</typeparam>
public class RavenDbRepository<T> : IRepository<T> where T : class
{
    private readonly IAsyncDocumentSession _session;

    /// <summary>
    /// Builds a new <see cref="RavenDbRepository{T}"/> instance.
    /// </summary>
    /// <param name="session">The underlying session that the repository will use to interact with documents.</param>
    public RavenDbRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }

    /// <inheritdoc/>
    public async Task<bool> All(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var negated = Expression.Lambda<Func<T, bool>>(Expression.Not(predicate.Body), predicate.Parameters);
        bool anyNegated = await _session.Query<T>().Where(negated).AnyAsync(cancellationToken).ConfigureAwait(false);
        return !anyNegated;
    }

    /// <inheritdoc/>
    public async Task<bool> All<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var negated = Expression.Lambda<Func<TResult, bool>>(Expression.Not(predicate.Body), predicate.Parameters);
        bool anyNegated = await _session.Query<T>()
            .Select(selector)
            .Where(negated)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);
        return !anyNegated;
    }

    /// <inheritdoc/>
    public virtual async Task<Page<T>> ReadPage(PageSize pageSize, PageIndex pageIndex,
        IEnumerable<IncludeClause<T>> includedProperties, IOrder<T> orderBy, CancellationToken ct = default)
    {
        IRavenQueryable<T> entries = _session.Query<T>();
        int total = await entries.CountAsync(ct)
            .ConfigureAwait(false);

        Page<T> pageOfResult = Page<T>.Empty(pageSize);

        if (total > 0)
        {
            IReadOnlyList<T> results = await entries.Include(includedProperties)
                .OrderBy(orderBy)
                .Skip(pageIndex < 1 ? 0 : ( pageIndex - 1 ) * pageSize)
                .Take(pageSize)
                .ToArrayAsync(ct)
                .ConfigureAwait(false);
            pageOfResult = new Page<T>(results, total, pageSize);
        }

        return pageOfResult;
    }

    /// <inheritdoc/>
    public async Task<bool> Any(CancellationToken cancellationToken = default)
        => await _session.Query<T>().AnyAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task Clear(CancellationToken ct = default)
    {
        IAsyncEnumerable<T> elements = _session.Query<T>().AsAsyncEnumerable();
        await foreach (T item in elements.WithCancellation(ct))
        {
            _session.Delete(item);
        }
    }

    /// <inheritdoc/>
    public async Task<int> Count(CancellationToken cancellationToken = default)
        => await _session.Query<T>().CountAsync(cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Asynchronously count the number of entries in the underlying repository that matches <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> Count(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _session.Query<T>().CountAsync(predicate, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<T> Create(T entry, CancellationToken cancellationToken = default)
    {
        await _session.StoreAsync(entry, cancellationToken);

        return entry;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<T>> Create(IEnumerable<T> entries, CancellationToken cancellationToken = default)
    {
        entries.ForEach(async entry => await _session.StoreAsync(entry, cancellationToken).ConfigureAwait(false));
        return Task.FromResult(entries);
    }

    /// <inheritdoc/>
    public async Task Delete(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        IAsyncEnumerable<T> entriesToDelete = Stream(predicate, ct);

        await foreach (T entry in entriesToDelete.WithCancellation(ct))
        {
            _session.Delete(entry);
        }
    }

    /// <inheritdoc/>
    public async Task<T> First(CancellationToken cancellationToken = default)
        => await First([], cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<T> First(IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default)
        => await _session.Query<T>()
            .Include(includedProperties)
            .FirstAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<T> First(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default)
        => await _session.Query<T>()
            .FirstAsync(predicate, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<TResult> First<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _session.Query<T>()
            .Where(predicate)
            .Select(selector)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Option<T>> FirstOrDefault(IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default)
        => ( await _session.Query<T>().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ).SomeNotNull();

    /// <inheritdoc/>
    public Task<Option<T>> FirstOrDefault(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> FirstOrDefault(Expression<Func<T, bool>> predicate,
        IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<TResult>> FirstOrDefault<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<TResult> Max<TResult>(Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<TResult> Min<TResult>(Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<T>> ReadAll(CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> ReadAll<TResult>(Expression<Func<T, TResult>> selector,
        CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<TResult>> ReadPage<TResult>(Expression<Func<T, TResult>> selector, PageSize pageSize,
        PageIndex page, IOrder<TResult> orderBy = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<TResult>> ReadPage<TResult>(Expression<Func<T, TResult>> selector, PageSize pageSize,
        PageIndex page, IOrder<T> orderBy = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<T> Single(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<T> Single(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<T> Single(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<TResult> Single<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> SingleOrDefault(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> SingleOrDefault(IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> SingleOrDefault(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> SingleOrDefault(Expression<Func<T, bool>> predicate,
        IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<TResult>> SingleOrDefault<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<TResult>> SingleOrDefault<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public IAsyncEnumerable<T> Stream(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TKey, TResult>(Expression<Func<T, bool>> predicate,
        Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TResult>> groupSelector,
        CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate, IOrder<T> orderBy = null,
        IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate, IOrder<TResult> orderBy = null,
        IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<TResult, bool>> predicate, IOrder<TResult> orderBy = null,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<T>> Where(Expression<Func<T, bool>> predicate, IOrder<T> orderBy, PageSize pageSize,
        PageIndex page, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>> predicate, IOrder<TResult> orderBy, PageSize pageSize, PageIndex pageIndex,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector,
        Expression<Func<TResult, bool>> predicate, IOrder<TResult> orderBy, PageSize pageSize, PageIndex pageIndex,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}