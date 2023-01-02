namespace Candoumbe.DataAccess.Repositories
{
    using Candoumbe.DataAccess.Abstractions;

    using DataFilters;

    using Microsoft.EntityFrameworkCore;

    using Optional;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Repository base class
    /// </summary>
    /// <typeparam name="TEntry">Type of entities the repository will manage.</typeparam>
    public abstract class RepositoryBase<TEntry> : IRepository<TEntry>
        where TEntry : class
    {
        /// <summary>
        /// <see cref="IDbContext"/> which the current instance operates on.
        /// </summary>
        protected IDbContext Context { get; }

        /// <summary>
        /// Builds a new <see cref="RepositoryBase{TEntry}"/> that handles <typeparamref name="TEntry"/>
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="ArgumentNullException">if <paramref name="context"/> is <see langword="null"/></exception>
        protected RepositoryBase(IDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc/>
        public virtual async ValueTask Clear(CancellationToken ct = default) => await Delete(_ => true, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, ISort<TResult> orderBy, CancellationToken ct = default)
        {
            DbSet<TEntry> entries = Context.Set<TEntry>();
            int total = await entries.CountAsync(ct)
                                     .ConfigureAwait(false);
            Page<TResult> pageOfResult = Page<TResult>.Empty(pageSize);
            if (total > 0)
            {
                IEnumerable<TResult> results = await entries.Select(selector)
                                                            .OrderBy(orderBy)
                                                            .Skip(page < 1 ? 0 : (page - 1) * pageSize)
                                                            .Take(pageSize)
                                                            .ToArrayAsync(ct)
                                                            .ConfigureAwait(false);
                pageOfResult = new Page<TResult>(results, total, pageSize);
            }

            return pageOfResult;
        }

        /// <inheritdoc/>
        public virtual async ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, ISort<TEntry> orderBy, CancellationToken ct = default)
        {
            DbSet<TEntry> entries = Context.Set<TEntry>();
            IQueryable<TEntry> resultQuery = entries;

            if (orderBy != default)
            {
                resultQuery = resultQuery.OrderBy(orderBy);
            }

            long total = await entries.CountAsync(ct).ConfigureAwait(false);

            Page<TResult> pageResult;

            if (total > 0)
            {
                IEnumerable<TResult> results = await resultQuery
                        .Skip(page < 1 ? 0 : (page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(selector)
                        .ToArrayAsync(ct)
                        .ConfigureAwait(false);

                pageResult = new Page<TResult>(results, total, pageSize);
            }
            else
            {
                pageResult = Page<TResult>.Empty(pageSize);
            }
            return pageResult;
        }

        /// <inheritdoc/>
        public virtual async ValueTask<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken ct = default)
            => await Context.Set<TEntry>().Select(selector)
                            .ToArrayAsync(ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<IEnumerable<TEntry>> ReadAllAsync(CancellationToken ct = default) => await ReadAllAsync(item => item, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => await Context.Set<TEntry>()
                            .Where(predicate)
                            .Select(selector)
                            .ToArrayAsync(ct)
                            .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<IEnumerable<TResult>> WhereAsync<TKey, TResult>(
            Expression<Func<TEntry, bool>> predicate,
            Expression<Func<TEntry, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupSelector,
            CancellationToken ct = default)
        {
            return predicate == null
                ? throw new ArgumentNullException(nameof(predicate))
                : keySelector == null
                ? throw new ArgumentNullException(nameof(keySelector))
                : groupSelector == null
                ? throw new ArgumentNullException(nameof(groupSelector))
                : (IEnumerable<TResult>)await Context.Set<TEntry>()
                                                     .Where(predicate)
                                                     .GroupBy(keySelector)
                                                     .Select(groupSelector)
                                                     .ToArrayAsync(ct)
                                                     .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => await WhereAsync(item => item, predicate, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate,
            ISort<TEntry> orderBy = null,
            IEnumerable<IncludeClause<TEntry>> includedProperties = null,
            CancellationToken ct = default) => await WhereAsync(
                item => item, predicate, orderBy, includedProperties, ct)
            .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                                                 Expression<Func<TEntry, bool>> predicate,
                                                                                 ISort<TResult> orderBy = null,
                                                                                 IEnumerable<IncludeClause<TEntry>> includedProperties = null,
                                                                                 CancellationToken ct = default)
            => await Context.Set<TEntry>()
                            .Where(predicate)
                            .Include(includedProperties)
                            .Select(selector)
                            .OrderBy(orderBy)
                            .ToArrayAsync(ct)
                            .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                                                 Expression<Func<TResult, bool>> predicate,
                                                                                 ISort<TResult> orderBy = null,
                                                                                 CancellationToken ct = default)
            => await Context.Set<TEntry>()
                            .Select(selector)
                            .Where(predicate)
                            .OrderBy(orderBy)
                            .ToArrayAsync(ct)
                            .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<Page<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, ISort<TEntry> orderBy, int pageSize, int page, CancellationToken ct = default)
        {
            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
            }
            IQueryable<TEntry> query = Context.Set<TEntry>()
                                              .Where(predicate)
                                              .OrderBy(orderBy)
                                              .Skip(pageSize * (page < 1 ? 1 : page - 1))
                                              .Take(pageSize);

            IEnumerable<TEntry> result = await query.ToListAsync(ct).ConfigureAwait(false);
            int total = await CountAsync(predicate, ct).ConfigureAwait(false);

            return new Page<TEntry>(result, total, pageSize);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<Page<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate,
            ISort<TResult> orderBy,
            int pageSize,
            int page,
            CancellationToken ct = default)
        {
            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
            }
            IOrderedQueryable<TResult> query = Context.Set<TEntry>()
                .Where(predicate)
                .Select(selector)
                .OrderBy(orderBy);

            IQueryable<TResult> results = query.Skip(pageSize * (page < 1 ? 1 : page - 1))
                .Take(pageSize);

            //we compute both ValueTask
            IEnumerable<TResult> result = await results.ToArrayAsync(ct)
                .ConfigureAwait(false);
            int total = await CountAsync(predicate, ct)
                .ConfigureAwait(false);

            return new Page<TResult>(result, total, pageSize);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<Page<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TResult, bool>> predicate,
            ISort<TResult> orderBy,
            int pageSize,
            int page,
            CancellationToken ct = default)
        {
            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
            }
            DbSet<TEntry> entries = Context.Set<TEntry>();
            IQueryable<TResult> query = entries.Select(selector)
                                               .Where(predicate)
                                               .OrderBy(orderBy)
                                               .Skip(pageSize * (page < 1 ? 1 : page - 1))
                                               .Take(pageSize);

            // we compute both ValueTask
            IEnumerable<TResult> result = await query.ToListAsync(ct)
                .ConfigureAwait(false);
            long total = await entries.Select(selector).LongCountAsync(predicate, ct)
                .ConfigureAwait(false);

            return total == 0
                ? Page<TResult>.Empty(pageSize)
                : new Page<TResult>(result, total, pageSize);
        }

        /// <inheritdoc/>
        public virtual async ValueTask<bool> AnyAsync(CancellationToken ct = default)
            => await Context.Set<TEntry>().AnyAsync(ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken ct = default)
            => await Context.Set<TEntry>().MaxAsync(selector, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken ct = default)
            => await Context.Set<TEntry>().MinAsync(selector, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => await Context.Set<TEntry>().AnyAsync(predicate, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<int> CountAsync(CancellationToken ct = default)
            => await Context.Set<TEntry>().CountAsync(ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<int> CountAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => await Context.Set<TEntry>().CountAsync(predicate, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TEntry> SingleAsync(CancellationToken ct = default)
            => await Context.Set<TEntry>().SingleAsync(ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => await Context.Set<TEntry>().SingleAsync(predicate, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate,
                                                           IEnumerable<IncludeClause<TEntry>> includes,
                                                           CancellationToken ct = default)
            => await Context.Set<TEntry>().Include(includes).SingleAsync(predicate, ct)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => await Context.Set<TEntry>().Where(predicate).Select(selector)
                .SingleAsync(ct)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TEntry>> SingleOrDefaultAsync(CancellationToken ct = default)
            => (await Context.Set<TEntry>().SingleOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TEntry>> SingleOrDefaultAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken ct = default)
            => (await Context.Set<TEntry>()
                .Include(includedProperties)
                .SingleOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TEntry>> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => (await Context.Set<TEntry>().SingleOrDefaultAsync(predicate, ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TEntry>> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken ct = default)
            => (await Context.Set<TEntry>()
                .Include(includedProperties)
                .SingleOrDefaultAsync(predicate, ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => (await Context.Set<TEntry>().Where(predicate)
                .Select(selector)
                .SingleOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken ct = default)
            => (await Context.Set<TEntry>()
                .Select(selector)
                .SingleOrDefaultAsync(predicate, ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<TEntry> FirstAsync(CancellationToken ct = default)
            => await FirstAsync(_ => true, ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await FirstAsync(predicate, Enumerable.Empty<IncludeClause<TEntry>>(), cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TEntry> FirstAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken ct = default)
            => await Context.Set<TEntry>().Include(includedProperties).FirstAsync(ct).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken ct = default)
            => await Context.Set<TEntry>().Include(includedProperties)
                            .FirstAsync(predicate, ct)
                            .ConfigureAwait(false);
        /// <inheritdoc/>
        public virtual async ValueTask<Option<TEntry>> FirstOrDefaultAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken ct = default)
            => (await Context.Set<TEntry>().Include(includedProperties)
                             .FirstOrDefaultAsync(ct)
                             .ConfigureAwait(false))
                             .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TEntry>> FirstOrDefaultAsync(CancellationToken ct = default)
            => await FirstOrDefaultAsync(Enumerable.Empty<IncludeClause<TEntry>>(), ct).ConfigureAwait(false);
        /// <inheritdoc/>
        public virtual async ValueTask<Option<TEntry>> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>().FirstOrDefaultAsync(cancellationToken)
                             .ConfigureAwait(false))
                             .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<TResult> FirstAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>()
                .Where(predicate)
                .Select(selector)
                .FirstAsync(cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TResult>> FirstOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => (await Context.Set<TEntry>()
                             .Where(predicate)
                             .Select(selector)
                             .FirstOrDefaultAsync(ct)
                             .ConfigureAwait(false))
                             .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async ValueTask<Option<TEntry>> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken ct = default)
            => (await Context.Set<TEntry>().Include(includedProperties)
                             .FirstOrDefaultAsync(predicate, ct)
                             .ConfigureAwait(false))
                             .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public abstract TEntry Create(TEntry entry);

        /// <inheritdoc/>
        public abstract ValueTask Delete(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default);

        /// <inheritdoc/>
        public abstract IEnumerable<TEntry> Create(IEnumerable<TEntry> entries);

        /// <inheritdoc/>
        public IAsyncEnumerable<TEntry> Stream(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Select(selector)
                          .Where(predicate)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, ISort<TResult> orderBy, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Select(selector)
                          .Where(predicate)
                          .OrderBy(orderBy)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, ISort<TResult> orderBy, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .Select(selector)
                          .OrderBy(orderBy)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .Select(selector)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual async ValueTask<bool> AllAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
            => await Context.Set<TEntry>()
                .AllAsync(predicate, ct)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async ValueTask<bool> AllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken ct = default)
            => await Context.Set<TEntry>()
                .Select(selector)
                .AllAsync(predicate, ct)
                .ConfigureAwait(false);
    }
}