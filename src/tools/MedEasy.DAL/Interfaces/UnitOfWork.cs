using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MedEasy.DAL.Repositories;
using System.Collections.Concurrent;

namespace MedEasy.DAL.Interfaces
{
    /// <summary>
    /// <para>
    /// Allow to work with several <see cref="IRepository{TEntry}"/>s.
    /// </para>
    /// Changes made to entities of any <see cref="IRepository{TEntry}"/> by calling the <see cref="Repository{TEntry}"/> method can either
    /// <list type="bullet">
    /// <item> be saved by calling <see cref="SaveChangesAsync"/>/<see cref="SaveChangesAsync(CancellationToken)"/></item>
    /// <item>disposed</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class UnitOfWork<TContext> : IUnitOfWork where TContext : IDbContext
    {
        private readonly TContext _context;
        private readonly IDictionary<Type, object> _repositories;
        private bool _disposed;

        /// <summary>
        /// Builds a new instance of <see cref="UnitOfWork{TContext}"/>
        /// </summary>
        /// <param name="context">instance of <see cref="TContext"/> that the current <see cref="UnitOfWork{TContext}"/> will wrap</param>
        public UnitOfWork(TContext context)
        {
            _context = context;
            _repositories = new ConcurrentDictionary<Type, object>();
            _disposed = false;
        }

        public IRepository<TEntry> Repository<TEntry>() where TEntry : class
        {
            IRepository<TEntry> repository;
            // Checks if the Dictionary Key contains the Type class
            if (_repositories.Keys.Contains(typeof(TEntry)))
            {
                // Return the repository for that Type class
                repository = _repositories[typeof(TEntry)] as IRepository<TEntry>;
            }
            else
            {
                // If the repository for that Type class doesn't exist, create it
                repository = new Repository<TEntry>(_context);
                // Add it to the dictionary
                _repositories.Add(typeof(TEntry), repository);
            }

            return repository;
        }


        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) 
            => await _context.SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        
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