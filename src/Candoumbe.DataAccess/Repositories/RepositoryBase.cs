namespace Candoumbe.DataAccess.Repositories
{
    using Candoumbe.DataAccess.Abstractions;
    using Candoumbe.Types.Numerics;

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
        public virtual async Task Clear(CancellationToken cancellationToken = default) => await Delete(_ => true, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<Page<TEntry>> ReadPage(PageSize pageSize, PageIndex page, IOrder<TEntry> orderBy, CancellationToken cancellationToken = default)
            => await ReadPage(pageSize, page, Enumerable.Empty<IncludeClause<TEntry>>(), orderBy, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<Page<TEntry>> ReadPage(PageSize pageSize, PageIndex page, IEnumerable<IncludeClause<TEntry>> includedProperties, IOrder<TEntry> orderBy, CancellationToken cancellationToken = default)
        {
            DbSet<TEntry> entries = Context.Set<TEntry>();
            NonNegativeInteger total = NonNegativeInteger.From(await entries.CountAsync(cancellationToken)
                                                                            .ConfigureAwait(false));
            Page<TEntry> pageOfResult = Page<TEntry>.Empty(pageSize);
            if (total > 0)
            {
                IEnumerable<TEntry> results = await entries.Include(includedProperties)
                                                            .OrderBy(orderBy)
                                                            .Skip((page - 1) * pageSize)
                                                            .Take(pageSize)
                                                            .ToArrayAsync(cancellationToken)
                                                            .ConfigureAwait(false);
                pageOfResult = new Page<TEntry>(results, total, pageSize);
            }

            return pageOfResult;
        }

        /// <inheritdoc/>
        public virtual async Task<Page<TResult>> ReadPage<TResult>(Expression<Func<TEntry, TResult>> selector, PageSize pageSize, PageIndex page, IOrder<TResult> orderBy, CancellationToken cancellationToken = default)
        {
            DbSet<TEntry> entries = Context.Set<TEntry>();
            NonNegativeInteger total = NonNegativeInteger.From(await entries.CountAsync(cancellationToken)
                                                         .ConfigureAwait(false));

            Page<TResult> pageOfResult = Page<TResult>.Empty(pageSize);

            if (total > 0)
            {
                IEnumerable<TResult> results = await entries.Select(selector)
                                                            .OrderBy(orderBy)
                                                            .Skip((page - 1) * pageSize)
                                                            .Take(pageSize)
                                                            .ToArrayAsync(cancellationToken)
                                                            .ConfigureAwait(false);
                pageOfResult = new Page<TResult>(results, total, pageSize);
            }

            return pageOfResult;
        }

        /// <inheritdoc/>
        public virtual async Task<Page<TResult>> ReadPage<TResult>(Expression<Func<TEntry, TResult>> selector, PageSize pageSize, PageIndex page, IOrder<TEntry> orderBy, CancellationToken cancellationToken = default)
        {
            DbSet<TEntry> entries = Context.Set<TEntry>();
            IQueryable<TEntry> resultQuery = entries;

            if (orderBy != default)
            {
                resultQuery = resultQuery.OrderBy(orderBy);
            }

            NonNegativeInteger total = NonNegativeInteger.From(await entries.CountAsync(cancellationToken).ConfigureAwait(false));

            Page<TResult> pageResult;

            if (total != NonNegativeInteger.Zero)
            {
                IEnumerable<TResult> results = await resultQuery
                        .Skip(page < 1 ? 0 : (page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(selector)
                        .ToArrayAsync(cancellationToken)
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
        public virtual async Task<IEnumerable<TResult>> ReadAll<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().Select(selector)
                            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TEntry>> ReadAll(CancellationToken cancellationToken = default) => await ReadAll(item => item, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>()
                            .Where(predicate)
                            .Select(selector)
                            .ToArrayAsync(cancellationToken)
                            .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TResult>> Where<TKey, TResult>(
            Expression<Func<TEntry, bool>> predicate,
            Expression<Func<TEntry, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupSelector,
            CancellationToken cancellationToken = default)
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
                                                     .ToArrayAsync(cancellationToken)
                                                     .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TEntry>> Where(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await Where(item => item, predicate, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TEntry>> Where(Expression<Func<TEntry, bool>> predicate,
                                                             IOrder<TEntry> orderBy = null,
                                                             IEnumerable<IncludeClause<TEntry>> includedProperties = null,
                                                             CancellationToken cancellationToken = default)
            => await Where(item => item, predicate, orderBy, includedProperties, cancellationToken)
            .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                                                 Expression<Func<TEntry, bool>> predicate,
                                                                                 IOrder<TResult> orderBy = null,
                                                                                 IEnumerable<IncludeClause<TEntry>> includedProperties = null,
                                                                                 CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>()
                            .Where(predicate)
                            .Include(includedProperties)
                            .Select(selector)
                            .OrderBy(orderBy)
                            .ToArrayAsync(cancellationToken)
                            .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<TResult>> Where<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                                                 Expression<Func<TResult, bool>> predicate,
                                                                                 IOrder<TResult> orderBy = null,
                                                                                 CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>()
                            .Select(selector)
                            .Where(predicate)
                            .OrderBy(orderBy)
                            .ToArrayAsync(cancellationToken)
                            .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<Page<TEntry>> Where(Expression<Func<TEntry, bool>> predicate, IOrder<TEntry> orderBy, PageSize pageSize, PageIndex page, CancellationToken cancellationToken = default)
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

            IEnumerable<TEntry> result = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
            NonNegativeInteger total = await Count(predicate, cancellationToken).ConfigureAwait(false);

            return new Page<TEntry>(result, total, pageSize);
        }

        /// <inheritdoc/>
        public virtual async Task<Page<TResult>> Where<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate,
            IOrder<TResult> orderBy,
            PageSize pageSize,
            PageIndex page,
            CancellationToken cancellationToken = default)
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

            //we compute both Task
            IEnumerable<TResult> result = await results.ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
            NonNegativeInteger total = await Count(predicate, cancellationToken)
                .ConfigureAwait(false);

            return new Page<TResult>(result, total, pageSize);
        }

        /// <inheritdoc/>
        public virtual async Task<Page<TResult>> Where<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                                          Expression<Func<TResult, bool>> predicate,
                                                                          IOrder<TResult> orderBy,
                                                                          PageSize pageSize,
                                                                          PageIndex page,
                                                                          CancellationToken cancellationToken = default)
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

            // we compute both Task
            IEnumerable<TResult> result = await query.ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            NonNegativeInteger total = NonNegativeInteger.From(await entries.Select(selector)
                                                                            .CountAsync(predicate, cancellationToken)
                                                                            .ConfigureAwait(false));

            return total == 0
                ? Page<TResult>.Empty(pageSize)
                : new Page<TResult>(result, total, pageSize);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Any(CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().AnyAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<TResult> Max<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().MaxAsync(selector, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<TResult> Min<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().MinAsync(selector, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<bool> Any(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().AnyAsync(predicate, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<NonNegativeInteger> Count(CancellationToken cancellationToken = default)
            => NonNegativeInteger.From(await Context.Set<TEntry>().CountAsync(cancellationToken).ConfigureAwait(false));

        /// <inheritdoc/>
        public virtual async Task<NonNegativeInteger> Count(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => NonNegativeInteger.From(await Context.Set<TEntry>().CountAsync(predicate, cancellationToken).ConfigureAwait(false));

        /// <inheritdoc/>
        public virtual async Task<TEntry> Single(CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().SingleAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<TEntry> Single(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().SingleAsync(predicate, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<TEntry> Single(Expression<Func<TEntry, bool>> predicate,
                                                           IEnumerable<IncludeClause<TEntry>> includedProperties,
                                                           CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().Include(includedProperties).SingleAsync(predicate, cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<TResult> Single<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                                     Expression<Func<TEntry, bool>> predicate,
                                                                     CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().Where(predicate).Select(selector)
                .SingleAsync(cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<Option<TEntry>> SingleOrDefault(CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>().SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<Option<TEntry>> SingleOrDefault(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>()
                .Include(includedProperties)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<Option<TEntry>> SingleOrDefault(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>().SingleOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<Option<TEntry>> SingleOrDefault(Expression<Func<TEntry, bool>> predicate,
                                                                            IEnumerable<IncludeClause<TEntry>> includedProperties,
                                                                            CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>()
                .Include(includedProperties)
                .SingleOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<Option<TResult>> SingleOrDefault<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                                                      Expression<Func<TEntry, bool>> predicate,
                                                                                      CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>().Where(predicate)
                .Select(selector)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<Option<TResult>> SingleOrDefault<TResult>(Expression<Func<TEntry, TResult>> selector,
                                                                                      Expression<Func<TResult, bool>> predicate,
                                                                                      CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>()
                .Select(selector)
                .SingleOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<TEntry> First(CancellationToken cancellationToken = default)
            => await First(_ => true, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<TEntry> First(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await First(predicate, Enumerable.Empty<IncludeClause<TEntry>>(), cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<TEntry> First(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().Include(includedProperties).FirstAsync(cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<TEntry> First(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>().Include(includedProperties)
                            .FirstAsync(predicate, cancellationToken)
                            .ConfigureAwait(false);
        /// <inheritdoc/>
        public virtual async Task<Option<TEntry>> FirstOrDefault(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellation = default)
            => (await Context.Set<TEntry>().Include(includedProperties)
                             .FirstOrDefaultAsync(cancellation)
                             .ConfigureAwait(false))
                             .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<Option<TEntry>> FirstOrDefault(CancellationToken cancellationToken = default)
            => await FirstOrDefault(Enumerable.Empty<IncludeClause<TEntry>>(), cancellationToken).ConfigureAwait(false);
        /// <inheritdoc/>
        public virtual async Task<Option<TEntry>> FirstOrDefault(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>().FirstOrDefaultAsync(cancellationToken)
                             .ConfigureAwait(false))
                             .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<TResult> First<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>()
                .Where(predicate)
                .Select(selector)
                .FirstAsync(cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<Option<TResult>> FirstOrDefault<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>()
                             .Where(predicate)
                             .Select(selector)
                             .FirstOrDefaultAsync(cancellationToken)
                             .ConfigureAwait(false))
                             .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public virtual async Task<Option<TEntry>> FirstOrDefault(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default)
            => (await Context.Set<TEntry>().Include(includedProperties)
                             .FirstOrDefaultAsync(predicate, cancellationToken)
                             .ConfigureAwait(false))
                             .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public abstract Task<TEntry> Create(TEntry entry, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task Delete(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<IEnumerable<TEntry>> Create(IEnumerable<TEntry> entries, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public IAsyncEnumerable<TEntry> Stream(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Select(selector)
                          .Where(predicate)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, IOrder<TResult> orderBy, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Select(selector)
                          .Where(predicate)
                          .OrderBy(orderBy)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IOrder<TResult> orderBy, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .Select(selector)
                          .OrderBy(orderBy)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .Select(selector)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual async Task<bool> All(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>()
                .AllAsync(predicate, cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public virtual async Task<bool> All<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default)
            => await Context.Set<TEntry>()
                .Select(selector)
                .AllAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
    }
}