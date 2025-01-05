namespace Candoumbe.DataAccess.RavenDb;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using DataFilters;
using Optional;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Repositories;

/// <summary>
/// RavenDb implementation of <see cref="IRepository{TEntry}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class RavenDbRepository<T> : IRepository<T> where T : class
{
    private readonly IAsyncDocumentSession _session;
    
    /// <summary>
    /// Builds a new <see cref="RavenDbRepository{T}"/> instance.
    /// </summary>
    /// <param name="session">The underlying document session that will be used to </param>
    public RavenDbRepository(IAsyncDocumentSession session)
    {
        _session = session;
    }
    /// <inheritdoc/>
    public async Task<bool> All(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        IRavenQueryable<T> query = _session.Query<T>().Where(predicate);

        return (await query.CountAsync(cancellationToken).ConfigureAwait(false)) == await _session.Query<T>().CountAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<bool> All<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default)
    {
        IRavenQueryable<TResult> query = _session.Query<T>().Select(selector).Where(predicate);
        return (await query.CountAsync(cancellationToken).ConfigureAwait(false)) == await _session.Query<T>().CountAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<Page<T>> ReadPage(PageSize pageSize, PageIndex page, IEnumerable<IncludeClause<T>> includedProperties, IOrder<T> orderBy, CancellationToken ct = default)
    {
        IRavenQueryable<T> entries = _session.Query<T>();
        int total = await entries.CountAsync(ct)
            .ConfigureAwait(false);

        Page<T> pageOfResult = Page<T>.Empty(pageSize);

        if (total > 0)
        {
            IEnumerable<T> results = await entries.Include(includedProperties)
                .OrderBy(orderBy)
                .Skip(page < 1 ? 0 : (page - 1) * pageSize)
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
    public async Task<bool> Any(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => await _session.Query<T>().AnyAsync(predicate, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task Clear(CancellationToken ct = default)
    {
        IAsyncEnumerable<T> elements = RavenQueryableExtensions.AsAsyncEnumerable(_session.Query<T>());
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
    public async Task<T> First(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => await _session.Query<T>()
            .Include(includedProperties)
            .FirstAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<T> First(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => await _session.Query<T>()
            .Include(includedProperties)
            .FirstAsync(predicate, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<TResult> First<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _session.Query<T>()
            .Where(predicate)
            .Select(selector)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Option<T>> FirstOrDefault(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => (await _session.Query<T>().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public async Task<Option<T>> FirstOrDefault(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => (await _session.Query<T>().FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public async Task<Option<T>> FirstOrDefault(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => (await _session.Query<T>().FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public async Task<Option<TResult>> FirstOrDefault<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) 
        => (await _session.Query<T>().Select(selector).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public Task<TResult> Max<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
    {
        IRavenQueryable<TResult> query = _session.Query<T>().Select(selector).OrderBy(x => x);
        return Task.FromResult(query.Last());
    }

    /// <inheritdoc/>
    public Task<TResult> Min<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
    {
        IRavenQueryable<TResult> query = _session.Query<T>().Select(selector).OrderBy(x => x);
        return Task.FromResult(query.First());
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> ReadAll(CancellationToken ct = default)
        => await _session.Query<T>().ToListAsync(ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IEnumerable<TResult>> ReadAll<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct = default)
        => await _session.Query<T>().Select(selector).ToListAsync(ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Page<TResult>> ReadPage<TResult>(Expression<Func<T, TResult>> selector, PageSize pageSize, PageIndex page, IOrder<TResult> orderBy = null, CancellationToken ct = default)
    {
        IRavenQueryable<TResult> query = _session.Query<T>().Select(selector);
        IEnumerable<TResult> items = orderBy is not null
            ? await query.OrderBy(orderBy).Skip(pageSize * (page - 1)).Take(pageSize).ToListAsync(ct).ConfigureAwait(false)
            : await query.Skip(pageSize * (page - 1)).Take(pageSize).ToListAsync(ct).ConfigureAwait(false);

        long total = await query.CountAsync(ct).ConfigureAwait(false);

       return new Page<TResult>(items, total, pageSize);
    }

    /// <inheritdoc/>
    public async Task<Page<TResult>> ReadPage<TResult>(Expression<Func<T, TResult>> selector, PageSize pageSize, PageIndex page, IOrder<T> orderBy = null, CancellationToken ct = default)
    {
        IRavenQueryable<T> query = _session.Query<T>();
        IEnumerable<TResult> items = orderBy is not null
            ? await query.OrderBy(orderBy).Select(selector).Skip(pageSize * (page - 1)).Take(pageSize).ToListAsync(ct).ConfigureAwait(false)
            : await query.Select(selector).Skip(pageSize * (page - 1)).Take(pageSize).ToListAsync(ct).ConfigureAwait(false);

        long total = await query.CountAsync(ct).ConfigureAwait(false);

        return new Page<TResult>(items, total, pageSize);
    }

    /// <inheritdoc/>
    public async Task<T> Single(CancellationToken cancellationToken = default)
        => await _session.Query<T>().SingleAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<T> Single(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _session.Query<T>().SingleAsync(predicate, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<T> Single(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => await _session.Query<T>().SingleAsync(predicate, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<TResult> Single<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) 
        => await _session.Query<T>().Where(predicate).Select(selector).SingleAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Option<T>> SingleOrDefault(CancellationToken cancellationToken = default)
        => (await _session.Query<T>().SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public async Task<Option<T>> SingleOrDefault(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => (await _session.Query<T>().SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public async Task<Option<T>> SingleOrDefault(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => (await _session.Query<T>().SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public async Task<Option<T>> SingleOrDefault(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
        => (await _session.Query<T>().SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public async Task<Option<TResult>> SingleOrDefault<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => (await _session.Query<T>().Where(predicate).Select(selector).SingleOrDefaultAsync( cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public async Task<Option<TResult>> SingleOrDefault<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default)
        => (await _session.Query<T>().Select(selector).Where(predicate).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false)).SomeNotNull();

    /// <inheritdoc/>
    public IAsyncEnumerable<T> Stream(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => Stream(ExpressionExtensions.Identity<T>(), predicate, ct);

    /// <inheritdoc/>
    public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => RavenQueryableExtensions.AsAsyncEnumerable(_session.Query<T>().Where(predicate).Select(selector));

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await Where(ExpressionExtensions.Identity<T>(), predicate, ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _session.Query<T>().Where(predicate).Select(selector).ToListAsync(ct).ConfigureAwait(false);

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TKey, TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TResult>> groupSelector, CancellationToken ct = default) 
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate, IOrder<T> orderBy = null, IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, IOrder<TResult> orderBy = null, IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, IOrder<TResult> orderBy = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    /// <inheritdoc/>
    public async Task<Page<T>> Where(Expression<Func<T, bool>> predicate, IOrder<T> orderBy, PageSize pageSize, PageIndex page, CancellationToken cancellationToken = default)
    {
        IRavenQueryable<T> query = _session.Query<T>().Where(predicate);
        long total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        IEnumerable<T> items = total switch
        {
            > 0 when orderBy is not null => await query.OrderBy(orderBy).Skip(pageSize * ( page - 1 )).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false),
            > 0 => await query.Skip(pageSize * ( page - 1 )).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false),
            _ => []
        };

        return new Page<T>(items, total, pageSize);
    }

    /// <inheritdoc/>
    public async Task<Page<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, IOrder<TResult> orderBy, PageSize pageSize, PageIndex page, CancellationToken cancellationToken = default)
    {
        IRavenQueryable<TResult> query = _session.Query<T>().Where(predicate).Select(selector);
        long total = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        IEnumerable<TResult> items = total switch
        {
            > 0 when orderBy is not null => await query.OrderBy(orderBy).Skip(pageSize * ( page - 1 )).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false),
            > 0 => await query.Skip(pageSize * ( page - 1 )).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false),
            _ => []
        };

        return new Page<TResult>(items, total, pageSize);
    }

    /// <inheritdoc/>
    public async Task<Page<TResult>> Where<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, IOrder<TResult> orderBy, PageSize pageSize, PageIndex page, CancellationToken cancellationToken = default)
    {
        IRavenQueryable<TResult> query = _session.Query<T>().Select(selector).Where(predicate);
        IEnumerable<TResult> items = orderBy is not null
            ? await query.OrderBy(orderBy).Skip(pageSize * (page - 1)).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false)
            : await query.Skip(pageSize * (page - 1)).Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);

        long total = await query.Where(predicate).CountAsync(cancellationToken).ConfigureAwait(false);

        return new Page<TResult>(items, total, pageSize);
    }
}