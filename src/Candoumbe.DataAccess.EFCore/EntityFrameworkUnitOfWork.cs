namespace Candoumbe.DataAccess.Abstractions
{
    using Microsoft.EntityFrameworkCore;

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// <para>
    /// Allow to work with several <see cref="IRepository{TEntry}"/> instances.
    /// </para>
    /// Changes made to entities of any <see cref="IRepository{TEntry}"/> by calling the <see cref="Repository{TEntry}"/> method can either
    /// <list type="bullet">
    /// <item> be saved by calling <see cref="SaveChangesAsync"/>/<see cref="SaveChangesAsync(CancellationToken)"/></item>
    /// <item>disposed</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TContext">Type of the context onto which the <see cref="EntityFrameworkUnitOfWork{TContext}"/></typeparam>
    public class EntityFrameworkUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext, IDbContext
    {
        private readonly TContext _context;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IDictionary<Type, object> _repositories;
        private bool _disposed;

        /// <summary>
        /// Builds a new instance of <see cref="EntityFrameworkUnitOfWork{TContext}"/>
        /// </summary>
        /// <param name="context">instance of <typeparamref name="TContext"/> that the current <see cref="EntityFrameworkUnitOfWork{TContext}"/> will wrap</param>
        /// <param name="repositoryFactory"></param>
        public EntityFrameworkUnitOfWork(TContext context, IRepositoryFactory repositoryFactory)
        {
            _context = context;
            _repositoryFactory = repositoryFactory;
            _repositories = new ConcurrentDictionary<Type, object>();
            _disposed = false;
        }

        /// <inheritdoc/>
        public IRepository<TEntry> Repository<TEntry>() where TEntry : class
        {
            IRepository<TEntry> repository;
            // Checks if the Dictionary Key contains the Type class
            if (!_repositories.TryGetValue(typeof(TEntry), out object value))
            {
                repository = value as IRepository<TEntry>;
            }
            else
            {
                // If the repository for that Type class doesn't exist, create it
                repository = _repositoryFactory.NewRepository<TEntry>(_context);
                // Add it to the dictionary
                _repositories.Add(typeof(TEntry), repository);
            }

            return repository;
        }

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        public async Task SaveChangesAsync(CancellationToken ct = default)
            => await _context.SaveChangesAsync(ct).ConfigureAwait(false);

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the current object
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}