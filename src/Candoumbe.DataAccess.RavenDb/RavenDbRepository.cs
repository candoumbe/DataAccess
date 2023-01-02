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
        private readonly IRavenQueryable<T> _entries;
        /// <inheritdoc/>
        public RavenDbRepository(IAsyncDocumentSession session)
        {
            _session = session;
            _entries = session.Query<T>();
        }
        /// <inheritdoc/>
        public async ValueTask<bool> AllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await _entries.AllAsync(predicate).ConfigureAwait(false);

        /// <inheritdoc/>
        public async ValueTask<bool> AllAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default)
            => await _entries
                .Select(selector)
                .AllAsync(predicate)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public async ValueTask<bool> AnyAsync(CancellationToken cancellationToken = default) => await _entries.AnyAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public ValueTask<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public async ValueTask Clear(CancellationToken ct = default)
        {
            IAsyncEnumerable<T> elements = _session.Query<T>().AsAsyncEnumerable();
            await foreach (T item in elements.WithCancellation(ct))
            {
                _session.Delete(item);
            }
        }
        /// <inheritdoc/>
        public async ValueTask<int> CountAsync(CancellationToken cancellationToken = default) => await _entries.CountAsync(cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Asynchronously count the number of entries in the underlying repository that matches <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await _entries.CountAsync(predicate, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public T Create(T entry) => throw new NotImplementedException();

        /// <inheritdoc/>
        public IEnumerable<T> Create(IEnumerable<T> entries)
        {
            entries.ForEach(async entry => await _session.StoreAsync(entry).ConfigureAwait(false));
            return entries;
        }

        /// <inheritdoc/>
        public ValueTask Delete(Expression<Func<T, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public async ValueTask<T> FirstAsync(CancellationToken cancellationToken = default)
            => await FirstAsync(Enumerable.Empty<IncludeClause<T>>(), cancellationToken).ConfigureAwait(false);


        /// <inheritdoc/>
        public async ValueTask<T> FirstAsync(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
            => await _entries.Include(includedProperties)
                             .FirstAsync(cancellationToken).ConfigureAwait(false);

#if NETSTANDARD2_0
        /// <inheritdoc/>
        public async ValueTask<T> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await FirstAsync(predicate, Enumerable.Empty<IncludeClause<T>>(), cancellationToken).ConfigureAwait(false);
#endif

        /// <inheritdoc/>
        public async ValueTask<T> FirstAsync(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
            => await _entries.Include(includedProperties).FirstAsync(predicate, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async ValueTask<TResult> FirstAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await _entries.Where(predicate)
                             .Select(selector)
                             .FirstAsync(cancellationToken)
                             .ConfigureAwait(false);

#if NETSTANDARD2_0
        /// <inheritdoc/>
        public async ValueTask<Option<T>> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
            => await FirstOrDefaultAsync(Enumerable.Empty<IncludeClause<T>>(), cancellationToken).ConfigureAwait(false);
#endif

        /// <inheritdoc/>
        public async ValueTask<Option<T>> FirstOrDefaultAsync(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
            => (await _entries.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)).SomeNotNull();

        /// <inheritdoc/>
        public ValueTask<Option<T>> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public ValueTask<Option<T>> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        /// <inheritdoc/>
        public ValueTask<Option<TResult>> FirstOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public ValueTask<TResult> MaxAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<TResult> MinAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<IEnumerable<T>> ReadAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<T, TResult>> selector, int pageSize, int page, ISort<TResult> orderBy = null, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<T, TResult>> selector, int pageSize, int page, ISort<T> orderBy = null, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<T> SingleAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<T> SingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public ValueTask<T> SingleAsync(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public ValueTask<TResult> SingleAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Option<T>> SingleOrDefaultAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Option<T>> SingleOrDefaultAsync(IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Option<T>> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Option<T>> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, IEnumerable<IncludeClause<T>> includedProperties, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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
        public ValueTask<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<IEnumerable<TResult>> WhereAsync<TKey, TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> keySelector, Expression<Func<IGrouping<TKey, T>, TResult>> groupSelector, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, ISort<T> orderBy = null, IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, ISort<TResult> orderBy = null, IEnumerable<IncludeClause<T>> includedProperties = null, CancellationToken ct = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, ISort<TResult> orderBy = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Page<T>> WhereAsync(Expression<Func<T, bool>> predicate, ISort<T> orderBy, int pageSize, int page, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Page<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate, ISort<TResult> orderBy, int pageSize, int page, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        /// <inheritdoc/>
        public ValueTask<Page<TResult>> WhereAsync<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<TResult, bool>> predicate, ISort<TResult> orderBy, int pageSize, int page, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
