using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
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


        
        
        public virtual async Task<IPagedResult<TEntry>> ReadPageAsync(IEnumerable<OrderClause<TEntry>> orderBy, int pageSize, int page)
        {
            return await ReadPageAsync(item => item, pageSize, page, orderBy)
                .ConfigureAwait(false);
        }

        public virtual async Task<IPagedResult<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, IEnumerable<OrderClause<TResult>> orderBy)
        {

            
            IQueryable<TResult> resultQuery = Entries.Select(selector);

            if (orderBy != null)
            {
                resultQuery = resultQuery.OrderBy(orderBy);
            }

            

            IEnumerable<TResult> results = await resultQuery
                .Skip(page < 1 ? 0 : (page - 1) * pageSize)
                .Take(pageSize)
                .ToArrayAsync()
                .ConfigureAwait(false);
            
            IPagedResult<TResult> pageResult = new PagedResult<TResult>(results, await Entries.CountAsync().ConfigureAwait(false), pageSize);

            return pageResult;
        }
        

        public virtual async Task<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector)
        {
            return await Entries.Select(selector).ToArrayAsync().ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TEntry>> ReadAllAsync()
        {
            return await ReadAllAsync(item => item).ConfigureAwait(false);
        }
        
        public virtual async Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate).Select(selector)
                .ToArrayAsync()
                .ConfigureAwait(false);
        }


        public async Task<IEnumerable<TResult>> WhereAsync<TKey, TResult>(Expression<Func<TEntry, bool>> predicate, Expression<Func<TEntry, TKey>> keySelector, Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupBySelector)
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
                    .ToArrayAsync()
                    .ConfigureAwait(false);
            
            return results;
        }



        public virtual async Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await WhereAsync(item => item, predicate);

        }

        public virtual async Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TEntry>> orderBy = null, IEnumerable<IncludeClause<TEntry>> includedProperties = null)
        {
            return await WhereAsync(item => item, predicate, orderBy, includedProperties).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy = null, IEnumerable<IncludeClause<TEntry>> includedProperties = null)
        {
            return await Entries
                .Where(predicate)
                .Include(includedProperties)
                .Select(selector)
                .OrderBy(orderBy)
                .ToArrayAsync()
                .ConfigureAwait(false);
        }

        public async Task<IPagedResult<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TEntry>> orderBy, int pageSize, int page)
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
            IEnumerable<TEntry> result = await query.ToListAsync();
            int total = await CountAsync(predicate);
            IPagedResult<TEntry> pagedResult = new PagedResult<TEntry>(result, total, pageSize);

            return pagedResult;
        }

        public async Task<IPagedResult<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy, int pageSize, int page)
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

        public async Task<IPagedResult<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy, int pageSize, int page)
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
            IEnumerable<TResult> result = await query.ToListAsync();
            int total = await Entries.Select(selector).CountAsync(predicate);
            IPagedResult<TResult> pagedResult = new PagedResult<TResult>(result, total, pageSize);

            return pagedResult;
        }

        public async Task<bool> AnyAsync()
        {
            return await Entries.AnyAsync().ConfigureAwait(false);
        }


        public async Task<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector)
        {
            return await Entries.MaxAsync(selector);
        }


        public async Task<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector)
        {
            return await Entries.MinAsync(selector).ConfigureAwait(false);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.AnyAsync(predicate).ConfigureAwait(false);
        }

        public async Task<int> CountAsync()
        {
            return await Entries.CountAsync().ConfigureAwait(false);
        }

        public async Task<int> CountAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.CountAsync(predicate)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> SingleAsync()
        {
            return await Entries.SingleAsync()
                .ConfigureAwait(false);
        }

        public async Task<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.SingleAsync(predicate)
                .ConfigureAwait(false);
        }


        public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate).Select(selector)
                .SingleAsync()
                .ConfigureAwait(false);
        }
        
        public async Task<TEntry> SingleOrDefaultAsync()
        {
            return await Entries.SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<TEntry> SingleOrDefaultAsync(IEnumerable<IncludeClause<TEntry>> includedProperties)
        {
            return await Entries
                .Include(includedProperties)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }


        public async Task<TEntry> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.SingleOrDefaultAsync(predicate)
                .ConfigureAwait(false);
        }



        public async Task<TEntry> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<IncludeClause<TEntry>> includedProperties)
        {
            return await Entries
                .Include(includedProperties)
                .SingleOrDefaultAsync(predicate)
                .ConfigureAwait(false);
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate)
                .Select(selector)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate)
        {
            return await Entries
                .Select(selector)
                .SingleOrDefaultAsync(predicate)
                .ConfigureAwait(false);
        }

        public async Task<TEntry> FirstAsync()
        {
            return await Entries.FirstAsync()
                .ConfigureAwait(false);
        }

        public async Task<TEntry> FirstOrDefaultAsync()
        {
            return await Entries.FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }


        public async Task<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.FirstAsync(predicate)
                .ConfigureAwait(false);
        }

        public void Delete(Expression<Func<TEntry, bool>> predicate)
        {
            IEnumerable<TEntry> entries = Entries.Where(predicate);
            Entries.RemoveRange(entries); 
        }


        public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries
                .Where(predicate)
                .Select(selector)
                .FirstAsync()
                .ConfigureAwait(false);
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate).Select(selector)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public async Task<TEntry> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.FirstOrDefaultAsync(predicate)
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

        public async Task<bool> AllAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries
                .AllAsync(predicate)
                .ConfigureAwait(false);
        }

        public async Task<bool> AllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate)
        {
            return await Entries
                .Select(selector)
                .AllAsync(predicate)
                .ConfigureAwait(false);
        }
    }
}