namespace MedEasy.DAL.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using MedEasy.DAL.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System.Threading;
    using Optional;
    using DataFilters;

    public class Repository<TEntry> : RepositoryBase<TEntry>, IRepository<TEntry> where TEntry : class
    {
        protected DbSet<TEntry> Entries { get; set; }

        /// <summary>
        /// Builds a new <see cref="Repository{TEntry}"/> instance
        /// </summary>
        /// <param name="context">the connection to use for the repository</param>
        public Repository(IDbContext context)
            : base(context)
        {
            Entries = Context.Set<TEntry>();
        }

        public void Clear() => Delete(_ => true);

        public virtual async ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, ISort<TResult> orderBy, CancellationToken ct = default)
        {
            int total = await Entries.CountAsync(ct)
                                     .ConfigureAwait(false);
            Page<TResult> pageOfResult = Page<TResult>.Empty(pageSize);
            if (total > 0)
            {
                IEnumerable<TResult> results = await Entries.Select(selector)
                                                            .OrderBy(orderBy)
                                                            .Skip(page < 1 ? 0 : (page - 1) * pageSize)
                                                            .Take(pageSize)
                                                            .ToArrayAsync(ct)
                                                            .ConfigureAwait(false);
                pageOfResult = new (results, total, pageSize);
            }

            return pageOfResult;
        }

        public virtual async ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, ISort<TEntry> orderBy, CancellationToken ct = default)
        {
            IQueryable<TEntry> resultQuery = Entries;

            if (orderBy != default)
            {
                resultQuery = resultQuery.OrderBy(orderBy);
            }

            long total = await Entries.CountAsync(ct)
                .ConfigureAwait(false);

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

        public virtual async ValueTask<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken ct = default) => await Entries.Select(selector).ToArrayAsync(ct).ConfigureAwait(false);

        public virtual async ValueTask<IEnumerable<TEntry>> ReadAllAsync(CancellationToken ct = default) => await ReadAllAsync(item => item, ct).ConfigureAwait(false);

        public virtual async ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries
                .Where(predicate)
                .Select(selector)
                .ToArrayAsync(ct)
                .ConfigureAwait(false);

        public async ValueTask<IEnumerable<TResult>> WhereAsync<TKey, TResult>(
            Expression<Func<TEntry, bool>> predicate,
            Expression<Func<TEntry, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupSelector,
            CancellationToken ct = default)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (groupSelector == null)
            {
                throw new ArgumentNullException(nameof(groupSelector));
            }

            return await Entries
                    .Where(predicate)
                    .GroupBy(keySelector)
                    .Select(groupSelector)
                    .ToArrayAsync(ct)
                    .ConfigureAwait(false);
        }

        public virtual async ValueTask<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await WhereAsync(item => item, predicate, ct)
            .ConfigureAwait(false);

        public virtual async ValueTask<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate,
            ISort<TEntry> orderBy = null,
            IEnumerable<IncludeClause<TEntry>> includedProperties = null,
            CancellationToken ct = default) => await WhereAsync(
                item => item, predicate, orderBy, includedProperties, ct)
            .ConfigureAwait(false);

        public virtual async ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate,
            ISort<TResult> orderBy = null,
            IEnumerable<IncludeClause<TEntry>> includedProperties = null,
            CancellationToken ct = default) => await Entries
                .Where(predicate)
                .Include(includedProperties)
                .Select(selector)
                .OrderBy(orderBy)
                .ToArrayAsync(ct)
                .ConfigureAwait(false);

        public virtual async ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(
           Expression<Func<TEntry, TResult>> selector,
           Expression<Func<TResult, bool>> predicate,
           ISort<TResult> orderBy = null,
           CancellationToken ct = default) => await Entries
               .Select(selector)
               .Where(predicate)
               .OrderBy(orderBy)
               .ToArrayAsync(ct)
               .ConfigureAwait(false);

        public async ValueTask<Page<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, ISort<TEntry> orderBy, int pageSize, int page, CancellationToken ct = default)
        {
            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
            }
            IQueryable<TEntry> query = Entries
                .Where(predicate)
                .OrderBy(orderBy)
                .Skip(pageSize * (page < 1 ? 1 : page - 1))
                .Take(pageSize);

            //we compute both ValueTask
            Page<TEntry> pagedResult;

            if (ct.IsCancellationRequested)
            {
                pagedResult = Page<TEntry>.Empty(pageSize);
            }
            else
            {
                IEnumerable<TEntry> result = await query.ToListAsync(ct)
                    .ConfigureAwait(false);
                int total = await CountAsync(predicate, ct)
                    .ConfigureAwait(false);
                pagedResult = new Page<TEntry>(result, total, pageSize);
            }

            return pagedResult;
        }

        public async ValueTask<Page<TResult>> WhereAsync<TResult>(
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
            IOrderedQueryable<TResult> query = Entries
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

        public async ValueTask<Page<TResult>> WhereAsync<TResult>(
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
            IQueryable<TResult> query = Entries.Select(selector)
                                               .Where(predicate)
                                               .OrderBy(orderBy)
                                               .Skip(pageSize * (page < 1 ? 1 : page - 1))
                                               .Take(pageSize);

            // we compute both ValueTask
            IEnumerable<TResult> result = await query.ToListAsync(ct)
                .ConfigureAwait(false);
            long total = await Entries.Select(selector).LongCountAsync(predicate, ct)
                .ConfigureAwait(false);

            return total == 0
                ? Page<TResult>.Empty(pageSize)
                : new Page<TResult>(result, total, pageSize);
        }

        public async ValueTask<bool> AnyAsync(CancellationToken ct = default) => await Entries.AnyAsync(ct).ConfigureAwait(false);

        public async ValueTask<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken ct = default) => await Entries.MaxAsync(selector, ct)
            .ConfigureAwait(false);

        public async ValueTask<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken ct = default) => await Entries.MinAsync(selector, ct).ConfigureAwait(false);

        public async ValueTask<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries.AnyAsync(predicate, ct).ConfigureAwait(false);

        public async ValueTask<int> CountAsync(CancellationToken ct = default) => await Entries.CountAsync(ct).ConfigureAwait(false);

        public async ValueTask<int> CountAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries.CountAsync(predicate, ct)
                .ConfigureAwait(false);

        public async ValueTask<TEntry> SingleAsync(CancellationToken ct = default) => await Entries.SingleAsync(ct)
                .ConfigureAwait(false);

        public async ValueTask<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries.SingleAsync(predicate, ct)
                .ConfigureAwait(false);

        public async ValueTask<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries.Where(predicate).Select(selector)
                .SingleAsync(ct)
                .ConfigureAwait(false);

        public async ValueTask<Option<TEntry>> SingleOrDefaultAsync(CancellationToken ct = default) => (await Entries.SingleOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        public async ValueTask<Option<TEntry>> SingleOrDefaultAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken ct = default) => (await Entries
                .Include(includedProperties)
                .SingleOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        public async ValueTask<Option<TEntry>> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => (await Entries.SingleOrDefaultAsync(predicate, ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        public async ValueTask<Option<TEntry>> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken ct = default) => (await Entries
                .Include(includedProperties)
                .SingleOrDefaultAsync(predicate, ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        public async ValueTask<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => (await Entries.Where(predicate)
                .Select(selector)
                .SingleOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        public async ValueTask<Option<TResult>> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken ct = default) => (await Entries
                .Select(selector)
                .SingleOrDefaultAsync(predicate, ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        public async ValueTask<TEntry> FirstAsync(CancellationToken ct = default) => await Entries.FirstAsync(ct)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public async ValueTask<Option<TEntry>> FirstOrDefaultAsync(CancellationToken ct = default) => (await Entries.FirstOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public async ValueTask<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries.FirstAsync(predicate, ct)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public void Delete(Expression<Func<TEntry, bool>> predicate)
        {
            IEnumerable<TEntry> entries = Entries.Where(predicate);
            Entries.RemoveRange(entries);
        }

        /// <inheritdoc/>
        public async ValueTask<TResult> FirstAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default) => await Entries
                .Where(predicate)
                .Select(selector)
                .FirstAsync(cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc/>
        public async ValueTask<Option<TResult>> FirstOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => (await Entries.Where(predicate).Select(selector)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public async ValueTask<Option<TEntry>> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => (await Entries.FirstOrDefaultAsync(predicate, ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        /// <inheritdoc/>
        public TEntry Create(TEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            return Entries.Add(entry).Entity;
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TEntry> Stream(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
        {
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken ct = default)
        {
            return Context.Set<TEntry>()
                          .Select(selector)
                          .Where(predicate)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, ISort<TResult> orderBy, CancellationToken ct = default)
        {
            return Context.Set<TEntry>()
                          .Select(selector)
                          .Where(predicate)
                          .OrderBy(orderBy)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, ISort<TResult> orderBy, CancellationToken ct = default)
        {
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .Select(selector)
                          .OrderBy(orderBy)
                          .AsAsyncEnumerable();
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<TResult> Stream<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default)
        {
            return Context.Set<TEntry>()
                          .Where(predicate)
                          .Select(selector)
                          .AsAsyncEnumerable();
        }


        public IEnumerable<TEntry> Create(IEnumerable<TEntry> entries)
        {
            Entries.AddRange(entries);

            return entries;
        }

        public async ValueTask<bool> AllAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries
                .AllAsync(predicate, ct)
                .ConfigureAwait(false);

        public async ValueTask<bool> AllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken ct = default) => await Entries
                .Select(selector)
                .AllAsync(predicate, ct)
                .ConfigureAwait(false);
    }
}