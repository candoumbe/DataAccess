using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading;
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




        public virtual async Task<IPagedResult<TEntry>> ReadPageAsync(IEnumerable<OrderClause<TEntry>> orderBy, int pageSize, int page, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ReadPageAsync(item => item, pageSize, page, orderBy, cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<IPagedResult<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, IEnumerable<OrderClause<TResult>> orderBy, CancellationToken cancellationToken = default(CancellationToken))
        {


            IQueryable<TResult> resultQuery = Entries.Select(selector);

            if (orderBy != null)
            {
                resultQuery = resultQuery.OrderBy(orderBy);
            }



            IEnumerable<TResult> results = await resultQuery
                .Skip(page < 1 ? 0 : (page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            IPagedResult<TResult> pageResult = new PagedResult<TResult>(results, await Entries.CountAsync(cancellationToken).ConfigureAwait(false), pageSize);

            return pageResult;
        }


        public virtual async Task<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.Select(selector).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TEntry>> ReadAllAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await ReadAllAsync(item => item, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries
                .Where(predicate)
                .Select(selector)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        public async Task<IEnumerable<TResult>> WhereAsync<TKey, TResult>(
            Expression<Func<TEntry, bool>> predicate,
            Expression<Func<TEntry, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupBySelector,
            CancellationToken cancellationToken = default(CancellationToken))
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
                    .ToArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

            return results;
        }



        public virtual async Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await WhereAsync(item => item, predicate, cancellationToken);

        }

        public virtual async Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate,
            IEnumerable<OrderClause<TEntry>> orderBy = null,
            IEnumerable<IncludeClause<TEntry>> includedProperties = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await WhereAsync(item => item, predicate, orderBy, includedProperties, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate,
            IEnumerable<OrderClause<TResult>> orderBy = null,
            IEnumerable<IncludeClause<TEntry>> includedProperties = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries
                .Where(predicate)
                .Include(includedProperties)
                .Select(selector)
                .OrderBy(orderBy)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IPagedResult<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TEntry>> orderBy, int pageSize, int page, CancellationToken cancellationToken = default(CancellationToken))
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

            //we compute both task
            IPagedResult<TEntry> pagedResult;

            if (cancellationToken.IsCancellationRequested)
            {
                pagedResult = PagedResult<TEntry>.Default;
            }
            else
            {
                IEnumerable<TEntry> result = await query.ToListAsync(cancellationToken);
                int total = await CountAsync(predicate, cancellationToken);
                pagedResult = new PagedResult<TEntry>(result, total, pageSize);

            }


            return pagedResult;
        }

        public async Task<IPagedResult<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector, 
            Expression<Func<TEntry, bool>> predicate, 
            IEnumerable<OrderClause<TResult>> orderBy, 
            int pageSize, 
            int page, 
            CancellationToken cancellationToken = default(CancellationToken))
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

            //we compute both task
            IEnumerable<TResult> result = await query.ToListAsync();
            int total = await CountAsync(predicate);
            IPagedResult<TResult> pagedResult = new PagedResult<TResult>(result, total, pageSize);

            return pagedResult;
        }

        public async Task<IPagedResult<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector, 
            Expression<Func<TResult, bool>> predicate, 
            IEnumerable<OrderClause<TResult>> orderBy, 
            int pageSize, 
            int page, 
            CancellationToken cancellationToken = default(CancellationToken))
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

            //we compute both task
            IPagedResult<TResult> pagedResult;
            if (! cancellationToken.IsCancellationRequested)
            {
                IEnumerable<TResult> result = await query.ToListAsync();
                int total = await Entries.Select(selector).CountAsync(predicate);
                pagedResult = new PagedResult<TResult>(result, total, pageSize);

            }
            else
            {
                pagedResult = PagedResult<TResult>.Default;
            }

            return pagedResult;
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.AnyAsync(cancellationToken).ConfigureAwait(false);
        }


        public async Task<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.MaxAsync(selector, cancellationToken);
        }


        public async Task<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.MinAsync(selector, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> CountAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.CountAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> SingleAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.SingleAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.SingleAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }


        public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.Where(predicate).Select(selector)
                .SingleAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> SingleOrDefaultAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> SingleOrDefaultAsync(IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries
                .Include(includedProperties)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        public async Task<TEntry> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.SingleOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }



        public async Task<TEntry> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries
                .Include(includedProperties)
                .SingleOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.Where(predicate)
                .Select(selector)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries
                .Select(selector)
                .SingleOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> FirstAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.FirstAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> FirstOrDefaultAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }


        public async Task<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.FirstAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }

        public void Delete(Expression<Func<TEntry, bool>> predicate)
        {
            IEnumerable<TEntry> entries = Entries.Where(predicate);
            Entries.RemoveRange(entries);
        }


        public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries
                .Where(predicate)
                .Select(selector)
                .FirstAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.Where(predicate).Select(selector)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries.FirstOrDefaultAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }


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

        public async Task<bool> AllAsync(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries
                .AllAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> AllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Entries
                .Select(selector)
                .AllAsync(predicate, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}