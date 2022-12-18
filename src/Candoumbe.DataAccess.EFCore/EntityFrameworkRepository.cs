namespace Candoumbe.DataAccess.EFStore
{
    using Candoumbe.DataAccess.Abstractions;
    using Candoumbe.DataAccess.Repositories;

    using Microsoft.EntityFrameworkCore;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Entity Framework implementation of <see cref="Repository{TEntry}"/> on top of <see cref="IDbContext"/>.
    /// </summary>
    /// <typeparam name="TEntry">Type of elements that the repository will handle</typeparam>
    /// <typeparam name="TContext">Type of the context the repository will u</typeparam>
    public class EntityFrameworkRepository<TEntry, TContext> : Repository<TEntry> where TEntry : class
        where TContext : DbContext, IDbContext
    {
        /// <summary>
        /// Builds a new <see cref="EntityFrameworkRepository{TEntry, TContext}"/> instance.
        /// </summary>
        /// <param name="context"></param>
        public EntityFrameworkRepository(TContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public override TEntry Create(TEntry entry) => ((DbContext)Context).Set<TEntry>().Add(entry).Entity;

        /// <inheritdoc/>
        public override IEnumerable<TEntry> Create(IEnumerable<TEntry> entries)
        {
            ((DbContext)Context).Set<TEntry>().AddRange(entries);

            return entries;
        }

        /// <inheritdoc/>
        public override async ValueTask Delete(Expression<Func<TEntry, bool>> predicate, CancellationToken cancellationToken = default)
        {
            IAsyncEnumerable<TEntry> entries = Entries.Where(predicate).AsAsyncEnumerable();
            await foreach (TEntry item in entries.WithCancellation(cancellationToken))
            {
                ((DbContext)Context).Set<TEntry>().Remove(item);
            }
        }

    }
}
