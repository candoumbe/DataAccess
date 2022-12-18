namespace Candoumbe.DataAccess.EFStore
{
    using Candoumbe.DataAccess.Abstractions;

    using Microsoft.EntityFrameworkCore;

    using System;

    /// <summary>
    /// Defines a unit of work factory wrapper around Entity Framework
    /// </summary>
    public class EntityFrameworkUnitOfWorkFactory<TContext> : UnitOfWorkFactory
        where TContext : DbContext, IDbContext
    {
        private readonly IRepositoryFactory _repositoryFactory;

        /// <summary>
        /// Options used to create the unit of work instances
        /// </summary>
        public DbContextOptions<TContext> Options { get; }

        /// <summary>
        /// Factory function for new <typeparamref name="TContext"/> instances.
        /// </summary>
        public Func<DbContextOptions<TContext>, TContext> ContextGenerator { get; }

        /// <summary>
        /// Builds a new <see cref="EntityFrameworkUnitOfWorkFactory{TContext}"/> instance
        /// </summary>
        /// <param name="options">options that will be used by the <see cref="EntityFrameworkUnitOfWork{TContext}"/> returned by calling <see cref="NewUnitOfWork"/></param>
        /// <param name="contextGenerator">Function to call to create new <typeparamref name="TContext"/> instances.</param>
        /// <param name="repositoryFactory">A factory that will be used to build repositories</param>
        /// <exception cref="ArgumentNullException">if either <paramref name="contextGenerator"/> or <paramref name="repositoryFactory"/> is <see langword="null"/>.</exception>
        public EntityFrameworkUnitOfWorkFactory(DbContextOptions<TContext> options, Func<DbContextOptions<TContext>, TContext> contextGenerator, IRepositoryFactory repositoryFactory)
        {
            Options = options;
            ContextGenerator = contextGenerator ?? throw new ArgumentNullException(nameof(contextGenerator));
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
        }

        /// <summary>
        /// Creates new <see cref="IUnitOfWork"/> instances.
        /// </summary>
        /// <remarks>
        /// Each call returns a new instance of <see cref="EntityFrameworkUnitOfWork{TContext}"/> (which wraps its own <see cref="DbContext"/> instance)
        /// that can safely be used in multithreaded fashion.
        /// </remarks>
        /// <returns><see cref="IUnitOfWork"/> instance</returns>
        public override IUnitOfWork NewUnitOfWork() => new EntityFrameworkUnitOfWork<TContext>(ContextGenerator.Invoke(Options), _repositoryFactory);
    }
}