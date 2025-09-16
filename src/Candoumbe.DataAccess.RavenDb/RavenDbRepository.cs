using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Candoumbe.DataAccess.Abstractions;
using Candoumbe.DataAccess.Repositories;
using Optional;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

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
    public async Task<bool> All(IFilterSpecification<T> predicate, CancellationToken cancellationToken = default)
    {
        Expression<Func<T, bool>> negated = Expression.Lambda<Func<T, bool>>(Expression.Not(predicate.Filter.Body), predicate.Filter.Parameters);
        bool anyNegated = await _session.Query<T>().Where(negated).AnyAsync(cancellationToken).ConfigureAwait(false);
        return !anyNegated;
    }

    /// <inheritdoc/>
    public async Task<bool> All<TResult>(IProjectionSpecification<T, TResult> selector,
        IFilterSpecification<TResult> predicate, CancellationToken cancellationToken = default)
    {
        Expression<Func<TResult, bool>> negated = Expression.Lambda<Func<TResult, bool>>(Expression.Not(predicate.Filter.Body), predicate.Filter.Parameters);
        bool anyNegated = await _session.Query<T>()
            .Select(selector)
            .Where(negated)
            .AnyAsync(cancellationToken)
            .ConfigureAwait(false);
        return !anyNegated;
    }

    /// <inheritdoc/>
    public virtual async Task<Page<T>> ReadPage(PageSize pageSize,
                                                PageIndex pageIndex,
                                                IEnumerable<IncludeClause<T>> includedProperties,
                                                IOrderSpecification<T> orderBy,
                                                CancellationToken ct = default)
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
    public Task<bool> Any(IFilterSpecification<T> predicate, CancellationToken cancellationToken = default) =>
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
    public async Task<int> Count(IFilterSpecification<T> predicate, CancellationToken cancellationToken = default)
        => await _session.Query<T>().CountAsync(predicate.Filter, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<T> Create(T entry, CancellationToken cancellationToken = default)
    {
        await _session.StoreAsync(entry, cancellationToken);

        return entry;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Create(IEnumerable<T> entries, CancellationToken cancellationToken = default)
    {
        await entries.ForEachAsync(async entry => await _session.StoreAsync(entry, cancellationToken).ConfigureAwait(false));
        return entries;
    }

    /// <inheritdoc/>
    public async Task Delete(IFilterSpecification<T> predicate, CancellationToken ct = default)
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
    public async Task<T> First(IFilterSpecification<T> predicate, IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default)
        => await _session.Query<T>()
            .FirstAsync(predicate.Filter, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<TResult> First<TResult>(IProjectionSpecification<T, TResult> selector,
        IFilterSpecification<T> predicate, CancellationToken cancellationToken = default)
        => await _session.Query<T>()
            .Where(predicate.Filter)
            .Select(selector)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Option<T>> FirstOrDefault(IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default)
        => ( await _session.Query<T>().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) ).SomeNotNull();

    /// <inheritdoc/>
    public Task<Option<T>> FirstOrDefault(IFilterSpecification<T> predicate,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> FirstOrDefault(IFilterSpecification<T> predicate,
        IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<TResult>> FirstOrDefault<TResult>(IProjectionSpecification<T, TResult> selector,
        IFilterSpecification<T> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<TResult> Max<TResult>(IProjectionSpecification<T, TResult> selector,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<TResult> Min<TResult>(IProjectionSpecification<T, TResult> selector,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<T>> ReadAll(CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> ReadAll<TResult>(IProjectionSpecification<T, TResult> selector,
        CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<TResult>> ReadPage<TResult>(IProjectionSpecification<T, TResult> selector, PageSize pageSize,
        PageIndex page, IOrderSpecification<TResult> orderBy = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<TResult>> ReadPage<TResult>(IProjectionSpecification<T, TResult> selector, PageSize pageSize,
        PageIndex page, IOrderSpecification<T> orderBy = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<T> Single(CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<T> Single(IFilterSpecification<T> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<T> Single(IFilterSpecification<T> predicate, IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<TResult> Single<TResult>(IProjectionSpecification<T, TResult> selector, IFilterSpecification<T> predicate,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> SingleOrDefault(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> SingleOrDefault(IEnumerable<IncludeClause<T>> includedProperties,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> SingleOrDefault(IFilterSpecification<T> predicate,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<T>> SingleOrDefault(IFilterSpecification<T> predicate,
        IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<TResult>> SingleOrDefault<TResult>(IProjectionSpecification<T, TResult> selector,
        IFilterSpecification<T> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Option<TResult>> SingleOrDefault<TResult>(IProjectionSpecification<T, TResult> selector,
        IFilterSpecification<TResult> predicate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public IAsyncEnumerable<T> Stream(IFilterSpecification<T> predicate, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> Stream<TResult>(IProjectionSpecification<T, TResult> selector,
        IFilterSpecification<T> predicate, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<T>> Where(IFilterSpecification<T> predicate, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TResult>(IProjectionSpecification<T, TResult> selector,
        IFilterSpecification<T> predicate, CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TKey, TResult>(IFilterSpecification<T> predicate,
        Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TResult>> groupSelector,
        CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<T>> Where(IFilterSpecification<T> predicate, IOrderSpecification<T> orderBy = null,
        IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TResult>(IProjectionSpecification<T, TResult> selector,
        IFilterSpecification<T> predicate, IOrderSpecification<TResult> orderBy = null,
        IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TResult>(IProjectionSpecification<T, TResult> selector,
                                                     IFilterSpecification<TResult> predicate,
                                                     IOrderSpecification<TResult> orderBy = null,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<T>> Where(IFilterSpecification<T> predicate, IOrderSpecification<T> orderBy, PageSize pageSize,
        PageIndex page, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<TResult>> Where<TResult>(IProjectionSpecification<T, TResult> selector,
                                              IFilterSpecification<T> predicate,
                                              IOrderSpecification<TResult> orderBy,
                                              PageSize pageSize,
                                              PageIndex pageIndex,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<Page<TResult>> Where<TResult>(IProjectionSpecification<T, TResult> selector,
                                              IFilterSpecification<TResult> predicate,
                                              IOrderSpecification<TResult> orderBy,
                                              PageSize pageSize,
                                              PageIndex pageIndex,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}