namespace Candoumbe.DataAccess.RavenDb
{
    using Candoumbe.DataAccess.Abstractions;
    using Candoumbe.DataAccess.Repositories;

    using DataFilters;

    using Microsoft.EntityFrameworkCore;

    using Optional;

    using Raven.Client.Documents.Linq;
    using Raven.Client.Documents.Session;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// RavenDb implementation of <see cref="IRepository{TEntry}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RavenDbRepository<T> : IRepository<T> where T : class
    {
        private readonly IAsyncDocumentSession _session;
        /// <inheritdoc/>
        public RavenDbRepository(IAsyncDocumentSession session)
        {
            _session = session;
        }
        /// <inheritdoc/>
        public async Task<bool> AllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await _session.Query<T>().AllAsync(predicate, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<bool> AllAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default)
            => await _session.Query<T>()
                .Select(selector)
                .AllAsync(predicate, cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<Page<T>> ReadPageAsync(PageSize pageSize, PageIndex page, IEnumerable<IncludeClause<T>> includedProperties, IOrder<T> orderBy, CancellationToken ct = default)
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
        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
            => await _session.Query<T>().AnyAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();

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
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
            => await _session.Query<T>().CountAsync(cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously count the number of entries in the underlying repository that matches <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
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
        public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
            => await FirstAsync(Enumerable.Empty<IncludeClause<T>>(), cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<T> FirstAsync(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
            => await _session.Query<T>()
                             .Include(includedProperties)
                             .FirstAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<T> FirstAsync(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
            => await _session.Query<T>()
                             .Include(includedProperties)
                             .FirstAsync(predicate, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<TResult> FirstAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await _session.Query<T>()
                             .Where(predicate)
                             .Select(selector)
                             .FirstAsync(cancellationToken)
                             .ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task<Option<T>> FirstOrDefaultAsync(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
            => (await _session.Query<T>().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)).SomeNotNull();

        /// <inheritdoc/>
        public Task<Option<T>> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<Option<T>> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<Option<TResult>> FirstOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<IEnumerable<T>> ReadAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<T, TResult>> selector, PageSize pageSize, PageIndex page, IOrder<TResult> orderBy = null, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<T, TResult>> selector, PageSize pageSize, PageIndex page, IOrder<T> orderBy = null, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<T> SingleAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<T> SingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<T> SingleAsync(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<TResult> SingleAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Option<T>> SingleOrDefaultAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Option<T>> SingleOrDefaultAsync(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Option<T>> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Option<T>> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public IAsyncEnumerable<T> Stream(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<IEnumerable<TResult>> WhereAsync<TKey, TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TResult>> groupSelector, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, IOrder<T> orderBy = null, IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, IOrder<TResult> orderBy = null, IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, IOrder<TResult> orderBy = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Page<T>> WhereAsync(Expression<Func<T, bool>> predicate, IOrder<T> orderBy, PageSize pageSize, PageIndex page, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Page<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, IOrder<TResult> orderBy, PageSize pageSize, PageIndex page, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public Task<Page<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, IOrder<TResult> orderBy, PageSize pageSize, PageIndex page, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
