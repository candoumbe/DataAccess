using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Optional;
#if NETSTANDARD1_3
using Z.EntityFramework.Plus;
#endif

namespace MedEasy.DAL.Repositories
{
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



        public void Clear() => Delete(x => true);

        public virtual async ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, IEnumerable<OrderClause<TResult>> orderBy, CancellationToken ct = default)
        {
            IQueryable<TResult> resultQuery = Entries.Select(selector);

            if (orderBy != null)
            {
                resultQuery = resultQuery.OrderBy(orderBy);
            }

            IEnumerable<TResult> results = await resultQuery
                .Skip(page < 1 ? 0 : (page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync(ct)
                .ConfigureAwait(false);

            Page<TResult> pageResult = new Page<TResult>(results, await Entries.CountAsync(ct).ConfigureAwait(false), pageSize);

            return pageResult;
        }

        public virtual async ValueTask<Page<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, IEnumerable<OrderClause<TEntry>> orderBy, CancellationToken ct = default)
        {
            IQueryable<TEntry> resultQuery = Entries
                .Skip(page < 1 ? 0 : (page - 1) * pageSize)
                .Take(pageSize);

            if (orderBy != default)
            {
                resultQuery = resultQuery.OrderBy(orderBy);
            }
                
            IEnumerable<TResult> results =    await resultQuery
                .Select(selector)
                .ToArrayAsync(ct)
                .ConfigureAwait(false);

            Page<TResult> pageResult = new Page<TResult>(results, await Entries.CountAsync(ct).ConfigureAwait(false), pageSize);

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
            Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupBySelector,
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
            if (groupBySelector == null)
            {
                throw new ArgumentNullException(nameof(groupBySelector));
            }

            IEnumerable<TResult> results = await Entries
                    .Where(predicate)
                    .GroupBy(keySelector)
                    .Select(groupBySelector)
                    .ToArrayAsync(ct)
                    .ConfigureAwait(false);

            return results;
        }



        public virtual async ValueTask<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await WhereAsync(item => item, predicate, ct);

        public virtual async ValueTask<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate,
            IEnumerable<OrderClause<TEntry>> orderBy = null,
            IEnumerable<IncludeClause<TEntry>> includedProperties = null,
            CancellationToken ct = default) => await WhereAsync(
                item => item, predicate, orderBy, includedProperties, ct)
            .ConfigureAwait(false);

        public virtual async ValueTask<IEnumerable<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate,
            IEnumerable<OrderClause<TResult>> orderBy = null,
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
           IEnumerable<OrderClause<TResult>> orderBy = null,
           CancellationToken ct = default) => await Entries
               .Select(selector)
               .Where(predicate)
               .OrderBy(orderBy)
               .ToArrayAsync(ct)
               .ConfigureAwait(false);

        public async ValueTask<Page<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TEntry>> orderBy, int pageSize, int page, CancellationToken ct = default)
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
                pagedResult = Page<TEntry>.Default;
            }
            else
            {
                IEnumerable<TEntry> result = await query.ToListAsync(ct);
                int total = await CountAsync(predicate, ct);
                pagedResult = new Page<TEntry>(result, total, pageSize);

            }

            return pagedResult;
        }

        public async ValueTask<Page<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector, 
            Expression<Func<TEntry, bool>> predicate, 
            IEnumerable<OrderClause<TResult>> orderBy, 
            int pageSize, 
            int page, 
            CancellationToken ct = default)
        {

            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
            }
            IQueryable<TResult> query = Entries
                .Where(predicate)
                .Select(selector)
                .OrderBy(orderBy)
                .Skip(pageSize * (page < 1 ? 1 : page - 1))
                .Take(pageSize);

            //we compute both ValueTask
            IEnumerable<TResult> result = await query.ToListAsync();
            int total = await CountAsync(predicate);
            Page<TResult> pagedResult = new Page<TResult>(result, total, pageSize);

            return pagedResult;
        }

        public async ValueTask<Page<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector, 
            Expression<Func<TResult, bool>> predicate, 
            IEnumerable<OrderClause<TResult>> orderBy, 
            int pageSize, 
            int page, 
            CancellationToken ct = default)
        {

            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
            }
            IQueryable<TResult> query = Entries
                .Select(selector)
                .Where(predicate)
                .OrderBy(orderBy)
                .Skip(pageSize * (page < 1 ? 1 : page - 1))
                .Take(pageSize);

            //we compute both ValueTask
            
            IEnumerable<TResult> result = await query.ToListAsync(ct)
                .ConfigureAwait(false);
            int total = await Entries.Select(selector).CountAsync(predicate, ct)
                .ConfigureAwait(false);
            Page<TResult> pagedResult = new Page<TResult>(result, total, pageSize);

            return pagedResult;
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

        public async ValueTask<Option<TEntry>> FirstOrDefaultAsync(CancellationToken ct = default) => (await Entries.FirstOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));


        public async ValueTask<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries.FirstAsync(predicate, ct)
                .ConfigureAwait(false);

        public void Delete(Expression<Func<TEntry, bool>> predicate)
        {
            IEnumerable<TEntry> entries = Entries.Where(predicate);
            Entries.RemoveRange(entries);
        }


        public async ValueTask<TResult> FirstAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => await Entries
                .Where(predicate)
                .Select(selector)
                .FirstAsync(ct)
                .ConfigureAwait(false);

        public async ValueTask<Option<TResult>> FirstOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => (await Entries.Where(predicate).Select(selector)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));

        public async ValueTask<Option<TEntry>> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken ct = default) => (await Entries.FirstOrDefaultAsync(predicate, ct)
                .ConfigureAwait(false))
                .NoneWhen(result => Equals(default, result));


        public TEntry Create(TEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            return Entries.Add(entry).Entity;
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